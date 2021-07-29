/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using NUnit.Framework;
using QuantConnect.Api;

namespace QuantConnect.Tests.API
{
    /// <summary>
    /// API Project endpoints, includes some Backtest endpoints testing as well 
    /// </summary>
    [TestFixture, Explicit("Requires configured api access and available backtest node to run on")]
    public class ProjectTests : ApiTestBase
    {
        /// <summary>
        /// Test creating and deleting projects with the Api
        /// </summary>
        [Test]
        public void Projects_CanBeCreatedAndDeleted_Successfully()
        {
            var name = "Test Project " + DateTime.Now.ToStringInvariant();

            //Test create a new project successfully
            var project = ApiClient.CreateProject(name, Language.CSharp, TestOrganization);
            Assert.IsTrue(project.Success);
            Assert.IsTrue(project.Projects.First().ProjectId > 0);
            Assert.IsTrue(project.Projects.First().Name == name);

            // Delete the project
            var deleteProject = ApiClient.DeleteProject(project.Projects.First().ProjectId);
            Assert.IsTrue(deleteProject.Success);

            // Make sure the project is really deleted
            var projectList = ApiClient.ListProjects();
            Assert.IsFalse(projectList.Projects.Any(p => p.ProjectId == project.Projects.First().ProjectId));
        }

        /// <summary>
        /// Test updating the files associated with a project
        /// </summary>
        [Test]
        public void CRUD_ProjectFiles_Successfully()
        {
            var fakeFile = new ProjectFile
            {
                Name = "Hello.cs",
                Code = HttpUtility.HtmlEncode("Hello, world!")
            };

            var realFile = new ProjectFile
            {
                Name = "main.cs",
                Code = HttpUtility.HtmlEncode(File.ReadAllText("../../../Algorithm.CSharp/BasicTemplateAlgorithm.cs"))
            };

            var secondRealFile = new ProjectFile()
            {
                Name = "algorithm.cs",
                Code = HttpUtility.HtmlEncode(File.ReadAllText("../../../Algorithm.CSharp/BubbleAlgorithm.cs"))
            };

            // Create a new project
            var project = ApiClient.CreateProject($"Test project - {DateTime.Now.ToStringInvariant()}", Language.CSharp, TestOrganization);
            Assert.IsTrue(project.Success);
            Assert.IsTrue(project.Projects.First().ProjectId > 0);

            // Add random file
            var randomAdd = ApiClient.AddProjectFile(project.Projects.First().ProjectId, fakeFile.Name, fakeFile.Code);
            Assert.IsTrue(randomAdd.Success);
            Assert.IsTrue(randomAdd.Files.First().Code == fakeFile.Code);
            Assert.IsTrue(randomAdd.Files.First().Name == fakeFile.Name);
            // Update names of file
            var updatedName = ApiClient.UpdateProjectFileName(project.Projects.First().ProjectId, randomAdd.Files.First().Name, realFile.Name);
            Assert.IsTrue(updatedName.Success);

            // Replace content of file
            var updateContents = ApiClient.UpdateProjectFileContent(project.Projects.First().ProjectId, realFile.Name, realFile.Code);
            Assert.IsTrue(updateContents.Success);

            // Read single file
            var readFile = ApiClient.ReadProjectFile(project.Projects.First().ProjectId, realFile.Name);
            Assert.IsTrue(readFile.Success);
            Assert.IsTrue(readFile.Files.First().Code == realFile.Code);
            Assert.IsTrue(readFile.Files.First().Name == realFile.Name);

            // Add a second file
            var secondFile = ApiClient.AddProjectFile(project.Projects.First().ProjectId, secondRealFile.Name, secondRealFile.Code);
            Assert.IsTrue(secondFile.Success);
            Assert.IsTrue(secondFile.Files.First().Code == secondRealFile.Code);
            Assert.IsTrue(secondFile.Files.First().Name == secondRealFile.Name);

            // Read multiple files
            var readFiles = ApiClient.ReadProjectFiles(project.Projects.First().ProjectId);
            Assert.IsTrue(readFiles.Success);
            Assert.IsTrue(readFiles.Files.Count == 4); // 2 Added + 2 Automatic (Research.ipynb & Main.cs)

            // Delete the second file
            var deleteFile = ApiClient.DeleteProjectFile(project.Projects.First().ProjectId, secondRealFile.Name);
            Assert.IsTrue(deleteFile.Success);

            // Read files
            var readFilesAgain = ApiClient.ReadProjectFiles(project.Projects.First().ProjectId);
            Assert.IsTrue(readFilesAgain.Success);
            Assert.IsTrue(readFilesAgain.Files.Count == 3);
            Assert.IsTrue(readFilesAgain.Files.Any(x => x.Name == realFile.Name));

            // Delete the project
            var deleteProject = ApiClient.DeleteProject(project.Projects.First().ProjectId);
            Assert.IsTrue(deleteProject.Success);
        }

        /// <summary>
        /// Test creating, compiling and backtesting a C# project via the Api
        /// </summary>
        [Test]
        public void CSharpProject_CreatedCompiledAndBacktested_Successully()
        {
            var language = Language.CSharp;
            var code = File.ReadAllText("../../../Algorithm.CSharp/BasicTemplateAlgorithm.cs");
            var algorithmName = "main.cs";
            var projectName = $"{DateTime.UtcNow.ToStringInvariant("u")} Test {TestAccount} Lang {language}";

            Perform_CreateCompileBackTest_Tests(projectName, language, algorithmName, code);
        }

        /// <summary>
        /// Test creating, compiling and backtesting a Python project via the Api
        /// </summary>
        [Test]
        public void PythonProject_CreatedCompiledAndBacktested_Successully()
        {
            var language = Language.Python;
            var code = File.ReadAllText("../../../Algorithm.Python/BasicTemplateAlgorithm.py");
            var algorithmName = "main.py";

            var projectName = $"{DateTime.UtcNow.ToStringInvariant("u")} Test {TestAccount} Lang {language}";

            Perform_CreateCompileBackTest_Tests(projectName, language, algorithmName, code);
        }

        private void Perform_CreateCompileBackTest_Tests(string projectName, Language language, string algorithmName, string code)
        {
            //Test create a new project successfully
            var project = ApiClient.CreateProject(projectName, language, TestOrganization);
            Assert.IsTrue(project.Success);
            Assert.IsTrue(project.Projects.First().ProjectId > 0);
            Assert.IsTrue(project.Projects.First().Name == projectName);

            // Make sure the project just created is now present
            var projects = ApiClient.ListProjects();
            Assert.IsTrue(projects.Success);
            Assert.IsTrue(projects.Projects.Any(p => p.ProjectId == project.Projects.First().ProjectId));

            // Test read back the project we just created
            var readProject = ApiClient.ReadProject(project.Projects.First().ProjectId);
            Assert.IsTrue(readProject.Success);
            Assert.IsTrue(readProject.Projects.First().Name == projectName);

            // Test set a project file for the project
            var file = new ProjectFile { Name = algorithmName, Code = code };
            var addProjectFile = ApiClient.AddProjectFile(project.Projects.First().ProjectId, file.Name, file.Code);
            Assert.IsTrue(addProjectFile.Success);

            // Download the project again to validate its got the new file
            var verifyRead = ApiClient.ReadProject(project.Projects.First().ProjectId);
            Assert.IsTrue(verifyRead.Success);

            // Compile the project we've created
            var compileCreate = ApiClient.CreateCompile(project.Projects.First().ProjectId);
            Assert.IsTrue(compileCreate.Success);
            Assert.IsTrue(compileCreate.State == CompileState.InQueue);

            // Read out the compile
            var compileSuccess = WaitForCompilerResponse(project.Projects.First().ProjectId, compileCreate.CompileId);
            Assert.IsTrue(compileSuccess.Success);
            Assert.IsTrue(compileSuccess.State == CompileState.BuildSuccess);

            // Update the file, create a build error, test we get build error
            file.Code += "[Jibberish at end of the file to cause a build error]";
            ApiClient.UpdateProjectFileContent(project.Projects.First().ProjectId, file.Name, file.Code);
            var compileError = ApiClient.CreateCompile(project.Projects.First().ProjectId);
            compileError = WaitForCompilerResponse(project.Projects.First().ProjectId, compileError.CompileId);
            Assert.IsTrue(compileError.Success); // Successfully processed rest request.
            Assert.IsTrue(compileError.State == CompileState.BuildError); //Resulting in build fail.

            // Using our successful compile; launch a backtest!
            var backtestName = $"{DateTime.Now.ToStringInvariant("u")} API Backtest";
            var backtest = ApiClient.CreateBacktest(project.Projects.First().ProjectId, compileSuccess.CompileId, backtestName);
            Assert.IsTrue(backtest.Success);

            // Now read the backtest and wait for it to complete
            var backtestRead = WaitForBacktestCompletion(project.Projects.First().ProjectId, backtest.BacktestId);
            Assert.IsTrue(backtestRead.Success);
            Assert.IsTrue(backtestRead.Progress == 1);
            Assert.IsTrue(backtestRead.Name == backtestName);
            Assert.IsTrue(backtestRead.Statistics["Total Trades"] == "1");
            Assert.IsTrue(backtestRead.Charts["Benchmark"].Series.Count > 0);

            // Verify we have the backtest in our project
            var listBacktests = ApiClient.ListBacktests(project.Projects.First().ProjectId);
            Assert.IsTrue(listBacktests.Success);
            Assert.IsTrue(listBacktests.Backtests.Count >= 1);
            Assert.IsTrue(listBacktests.Backtests[0].Name == backtestName);

            // Update the backtest name and test its been updated
            backtestName += "-Amendment";
            var renameBacktest = ApiClient.UpdateBacktest(project.Projects.First().ProjectId, backtest.BacktestId, backtestName);
            Assert.IsTrue(renameBacktest.Success);
            backtestRead = ApiClient.ReadBacktest(project.Projects.First().ProjectId, backtest.BacktestId);
            Assert.IsTrue(backtestRead.Name == backtestName);

            //Update the note and make sure its been updated:
            var newNote = DateTime.Now.ToStringInvariant("u");
            var noteBacktest = ApiClient.UpdateBacktest(project.Projects.First().ProjectId, backtest.BacktestId, note: newNote);
            Assert.IsTrue(noteBacktest.Success);
            backtestRead = ApiClient.ReadBacktest(project.Projects.First().ProjectId, backtest.BacktestId);
            Assert.IsTrue(backtestRead.Note == newNote);

            // Delete the backtest we just created
            var deleteBacktest = ApiClient.DeleteBacktest(project.Projects.First().ProjectId, backtest.BacktestId);
            Assert.IsTrue(deleteBacktest.Success);

            // Test delete the project we just created
            var deleteProject = ApiClient.DeleteProject(project.Projects.First().ProjectId);
            Assert.IsTrue(deleteProject.Success);
        }

        /// <summary>
        /// Wait for the compiler to respond to a specified compile request
        /// </summary>
        /// <param name="projectId">Id of the project</param>
        /// <param name="compileId">Id of the compilation of the project</param>
        /// <returns></returns>
        private Compile WaitForCompilerResponse(int projectId, string compileId)
        {
            var compile = new Compile();
            var finish = DateTime.Now.AddSeconds(60);
            while (DateTime.Now < finish)
            {
                compile = ApiClient.ReadCompile(projectId, compileId);
                if (compile.State == CompileState.BuildSuccess) break;
                Thread.Sleep(1000);
            }
            return compile;
        }

        /// <summary>
        /// Wait for the backtest to complete
        /// </summary>
        /// <param name="projectId">Project id to scan</param>
        /// <param name="backtestId">Backtest id previously started</param>
        /// <returns>Completed backtest object</returns>
        private Backtest WaitForBacktestCompletion(int projectId, string backtestId)
        {
            var result = new Backtest();
            var finish = DateTime.Now.AddSeconds(60);
            while (DateTime.Now < finish)
            {
                result = ApiClient.ReadBacktest(projectId, backtestId);
                if (result.Progress == 1) break;
                if (!result.Success) break;
                Thread.Sleep(1000);
            }
            return result;
        }
    }
}
