using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TextIsLoaded()
        {
            var task = Task.Run(async () => {
                return await LoadingSystem.Model.BusinessLogic
                             .ReadFromFileAsync(@"C:\TempFolder\LoadingSystem\LoadingSystem\Resources\104_ÄÌÍ_791 - Original.UT");
            });

            var text = task.Result;

            Assert.IsNotNull(text);
        }



        [TestMethod]
        public void CorrectCountOfColumns()
        {
            var task = Task.Run(async () => {
                return await LoadingSystem.Model.BusinessLogic
                             .ReadFromFileAsync(@"D:\Visual Studio Projects\C#\LoadingSystem\LoadingSystem\Resources\104_ÄÌÍ_791.UT");
            });

            var text = LoadingSystem.Model.BusinessLogic.InfoStartsFrom(task.Result, 6);

            var columns = LoadingSystem.Model.BusinessLogic.CountOfColumns(text);

            Assert.AreEqual(3, columns);
        }



        [TestMethod]
        public void CorrectCountOfRows()
        {
            var task = Task.Run(async () => {
                return await LoadingSystem.Model.BusinessLogic
                             .ReadFromFileAsync(@"D:\Visual Studio Projects\C#\LoadingSystem\LoadingSystem\Resources\104_ÄÌÍ_791.UT");
            });

            var text = LoadingSystem.Model.BusinessLogic.InfoStartsFrom(task.Result, 6);

            var rows = LoadingSystem.Model.BusinessLogic.CountOfRows(text);

            Assert.AreEqual(191, rows);
        }

        [TestMethod]
        public void CorrectDecimalCount()
        {
            var task = Task.Run(async () => {
                return await LoadingSystem.Model.BusinessLogic
                             .ReadFromFileAsync(@"D:\Visual Studio Projects\C#\LoadingSystem\LoadingSystem\Resources\104_ÄÌÍ_791.UT");
            });

            var text = LoadingSystem.Model.BusinessLogic.InfoStartsFrom(task.Result, 6);

            var count = LoadingSystem.Model.BusinessLogic.GetDecimalNumberCount(text, '.', ',');

            Assert.AreEqual(4, count);
        }
    }
}
