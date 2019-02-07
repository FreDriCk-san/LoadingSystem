using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LoadingSystem.Model
{
    public static class FileReader
    {
        private const int DefaultBufferSize = 4096;

        private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;

        public static async Task<string[]> ReadAllLinesAsync(string path)
        {
            var lines = new List<string>();
            var index = 1;

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(stream, CodepageDetector.getCyrillic(path)))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    lines.Add($"{index}:\t\t{line}");
                    index++;
                }
            }

            return lines.ToArray();
        }
    }

    class CodepageDetector
    {
        public CodepageDetector() { }

        static string[] mCodePages = { "windows-1251", "utf-8", "cp866", "koi8-r" };
        static string mDefaultPage = "us-ascii";
        static Decoder[] mDec = null;
        static char[] mAlphabet = {
            'А','Б','В','Г','Д','Е','Ё','Ж','З','И','Й','К','Л','М','Н','О','П','Р','С','Т','У','Ф','Х','Ц','Ч','Ш','Щ','Ъ','Ы','Ь','Э','Ю','Я',
            'а','б','в','г','д','е','ё','ж','з','и','й','к','л','м','н','о','п','р','с','т','у','ф','х','ц','ч','ш','щ','ъ','ы','ь','э','ю','я' };

        static public int getCyrillicQuality(string aStr)
        {
            int result = 0;
            int id = 0;
            while (id < aStr.Length)
            {
                if (aStr.IndexOfAny(mAlphabet, id, 1) >= 0)
                    result++;
                else if (aStr[id] > 127)
                    result--;
                id++;
            }
            return result;
        }
        public static Encoding getCyrillicFromBuffer(byte[] aBuff, int aIndex, int aSize)
        {
            if (mDec == null)
            {
                mDec = new Decoder[mCodePages.Length];
                int id = 0;
                foreach (string i in mCodePages)
                    mDec[id++] = Encoding.GetEncoding(i).GetDecoder();
            }

            int[] counts = new int[mCodePages.Length];
            for (int i = 0; i < mCodePages.Length; i++)
                counts[i] = 0;

            char[] chars = new char[aSize * 2];

            for (int i = 0; i < mCodePages.Length; i++)
            {
                mDec[i].GetChars(aBuff, aIndex, aSize, chars, 0);
                string s = new string(chars);
                counts[i] += getCyrillicQuality(s);
            }

            int idMax = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] > counts[idMax])
                    idMax = i;
            }

            return Encoding.GetEncoding(mCodePages[idMax]);
        }
        public static Encoding getCyrillic(string aFilename)
        {
            byte[] buff = new byte[1024 * 8];
            int size = 0;
            try
            {
                FileStream reader = new FileStream(aFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                size = reader.Read(buff, 0, buff.Length);
                reader.Close();
            }
            catch
            {
                return Encoding.GetEncoding(mDefaultPage);
            }
            if (size <= 0)
                return Encoding.GetEncoding(mDefaultPage);
            // detect BOM
            if (size >= 2)
            {
                if ((buff[0] == 0xff) && (buff[1] == 0xfe))
                    return Encoding.GetEncoding("utf-16");
                if ((buff[0] == 0xfe) && (buff[1] == 0xff))
                    return Encoding.GetEncoding("utf-16BE");

            }
            if ((size >= 3) && (buff[0] == 0xef) && (buff[1] == 0xbb) && (buff[2] == 0xbf))
                return Encoding.GetEncoding("utf-8");

            return getCyrillicFromBuffer(buff, 0, size);
        }
    }

    public class DynamicEncoder
    {
        Encoding mEnc = null;
        Encoding mCandidate = null;
        int mCandidateSuccess = 0;
        public string GetString(byte[] aBuff, int aIndex, int aSize)
        {
            if (mEnc != null)
                return mEnc.GetString(aBuff, aIndex, aSize);
            bool isAscii = true;
            for (int i = 0; (i < aSize) && isAscii; i++)
                isAscii = aBuff[aIndex + i] < 128;
            if (isAscii)
                return Encoding.ASCII.GetString(aBuff, aIndex, aSize);
            Encoding newCandidate = CodepageDetector.getCyrillicFromBuffer(aBuff, aIndex, aSize);
            string result = newCandidate.GetString(aBuff, aIndex, aSize);
            bool setCandidate = true;
            if (mCandidate != null)
            {
                string oldResult = mCandidate.GetString(aBuff, aIndex, aSize);
                int newQuality = CodepageDetector.getCyrillicQuality(result);
                int oldQuality = CodepageDetector.getCyrillicQuality(oldResult);
                if (newQuality <= oldQuality)
                {
                    mCandidateSuccess++;
                    setCandidate = false;
                    result = oldResult;
                }
            }
            if (setCandidate)
            {
                mCandidate = newCandidate;
                mCandidateSuccess = 0;
            }
            else if (mCandidateSuccess >= 3)
                mEnc = mCandidate;
            return result;
        }
    }
}
