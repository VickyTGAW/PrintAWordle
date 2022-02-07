using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.IO;

namespace WordleModel.Controllers
{
    public class WordleController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            //Let's take the data and convert it to the argument for the OpenSCAD
            string wordleResult = form["WordleResult"];
            string modelMode = form["ModelMode"];
            bool includeUnusedResults = false;
            double colorHeight = Double.Parse(form["ColorHeight"]);
            
            if (form.AllKeys.Contains("IncludeUnusedGuesses"))
                includeUnusedResults = form["IncludeUnusedGuesses"].ToString().ToUpper() == "ON";

            //For easy splitting, I'm replacing the carriage returns with a Pipe character
            wordleResult = wordleResult.Replace("\r\n", "|");

            //Is this a WordleResult?
            if (includesNoWordleCharacters(wordleResult))
            {
                Log("#############Odd Data#################");
                Log(wordleResult);
                Log("######################################");

                ViewBag.Message = "The result you entered does not appear to be from Wordle.";
                return View();
            }

            string resultTitle = "";

            double cubeSize = 3;
            double margin = 0.75;
            bool includeBacking = true;
            double boardThickness = 1;

            string stringForOpenSCAD = "[";
            string[] lines = wordleResult.Split('|');

            int guesses = 0;
            if (modelMode=="Earring")
            {
                cubeSize = 3;
                margin = 0.75;
                boardThickness = 1;
            }
            if (modelMode=="Pendant")
            {
                cubeSize = 3;
                margin = 0.75;
                boardThickness = 2;
            }
            if (modelMode=="Magnet")
            {
                cubeSize = 7;
                margin = 1;
                boardThickness = 2;
                includeBacking = false;
            }

            //20220205 - Setting a default Result Title
            resultTitle = String.Format("Wordle_{0}", modelMode);

            foreach (string line in lines)
            {
                string newLine = line;
                if (includesNoWordleCharacters(line))
                {
                    //We are going to mostly skip.  But if it is our first line, then we will use that for our fileName
                    //                    Wordle 224 4 / 6
                    if (line.IndexOf("Wordle") != -1)
                        resultTitle = String.Format("Wordle_{0}_{1}", modelMode, line.Replace("Wordle ", "").Replace(" ", "_").Replace("/", "_"));

                    continue;
                }
                    

                guesses++;
                //We want to make an array formatted like:
                //[[0.5,0.5,0.5,0.5,0.5],[1,1,0.5,0.5,0.5],[1.5,1,1,1,1.5],[1.5,1.5,1.5,1.5,1.5]]

                //If ⬜ or ⬛, we put in Level 1 Height
                //If \U0001f7e8, we put in Level 2 Height
                //If \U0001f7e9, we put in Level 3 Height
                newLine = newLine.Replace("⬜", colorHeight.ToString() + ",");
                newLine = newLine.Replace("⬛", colorHeight.ToString() +", ");

                newLine = newLine.Replace("\U0001f7e8", (colorHeight*2).ToString() + ",");
                newLine = newLine.Replace("\U0001f7e6", (colorHeight * 2).ToString() + ",");
                newLine = newLine.Replace("Cuadrado amarillo", (colorHeight * 2).ToString() + ",");
                newLine = newLine.Replace("Carré jaune", (colorHeight * 2).ToString() + ",");
                newLine = newLine.Replace("Yellow square", (colorHeight * 2).ToString() + ",");

                newLine = newLine.Replace("\U0001f7e9", (colorHeight * 3).ToString() + ",");
                newLine = newLine.Replace("\U0001f7e7", (colorHeight * 3).ToString() + ",");
                newLine = newLine.Replace("Cuadrado verde", (colorHeight * 3).ToString() + ",");
                newLine = newLine.Replace("Carré vert", (colorHeight * 3).ToString() + ",");
                newLine = newLine.Replace("Green square", (colorHeight * 3).ToString() + ",");

                //Sluff off the last comma
                newLine = newLine.Substring(0, newLine.Length - 1);

                if (stringForOpenSCAD != "[")
                    stringForOpenSCAD += ",";
                stringForOpenSCAD = stringForOpenSCAD + "[" + newLine + "]";

            }
            stringForOpenSCAD += "]";

            string fileName = String.Format("{0}_{1:yyyyMMdd_hhmmssfff}.stl", resultTitle, System.DateTime.Now);
            fileName = ReplaceInvalidChars(fileName);

            string openSCADCodeFile = System.Configuration.ConfigurationManager.AppSettings["CodeDirectory"] + "Wordle.scad";
            string fullOutgoingFilePath = System.Configuration.ConfigurationManager.AppSettings["ModelDirectory"] + fileName;

            string openSCADExecutable = System.Configuration.ConfigurationManager.AppSettings["OpenSCADExecutable"];


            try
            {
                FileStreamResult returnMe;
                
                //Run our OpenSCAD Command Line
                Process proc = Process.Start(openSCADExecutable, String.Format("-o {0} -D \"guesses={1}\" -D \"includeUnusedGuesses={2}\" -D \"includeBacking={3}\" -D \"cubeSize={4}\" -D \"margin={5}\" -D \"boardThickness={6}\" -D \"letters={7}\" {8}", fullOutgoingFilePath, guesses, includeUnusedResults.ToString().ToLower(), includeBacking.ToString().ToLower(), cubeSize, margin, boardThickness, stringForOpenSCAD, openSCADCodeFile));
                proc.WaitForExit();

                //Read the file OpenSCAD outputted and stream to the browser
                MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes(fullOutgoingFilePath));
                returnMe = new FileStreamResult(ms, "model/stl" );
                returnMe.FileDownloadName = fileName;
                return returnMe;
            }
            catch (SystemException e)
            {
                ViewBag.Message = "Unfortunately there was an error loading your 3D Model.  Your input has been saved to a log file and will be reviewed for future improvements. Thank you for helping identify issues.";
                Log("########ERROR PROCESSING MODEL########");
                Log("Error:" + e.Message);
                if (e.InnerException != null)
                    Log("Inner Exception: " + e.InnerException);
                Log("WordleResult: " + wordleResult);
                Log("OpenSCAD Command Line" + String.Format("-o {0} -D \"guesses={1}\" -D \"includeUnusedGuesses={2}\" -D \"includeBacking={3}\" -D \"cubeSize={4}\" -D \"margin={5}\" -D \"boardThickness={6}\" -D \"letters={7}\" {8}", fullOutgoingFilePath, guesses, includeUnusedResults.ToString().ToLower(), includeBacking.ToString().ToLower(), cubeSize, margin, boardThickness, stringForOpenSCAD, openSCADCodeFile));
                Log("#####################################");

            }
           

            return View();
        }

        private void Log(string message)
        {
            string logDirectory = System.Configuration.ConfigurationManager.AppSettings["LogFile"].ToString();
            if (System.IO.File.Exists(logDirectory)) // write to the log
            {
                using (StreamWriter r = new StreamWriter(logDirectory, true))
                {
                    r.WriteLine(DateTime.Now + ": " + message);
                }
            }
            else
            {
                using (StreamWriter r = System.IO.File.CreateText(logDirectory)) // creates a log if there isn't an active one
                {
                    r.WriteLine(DateTime.Now + ": " + message);
                }
            }
           
        }

        public string ReplaceInvalidChars(string filename)
        {
            return string.Join("", filename.Split(Path.GetInvalidFileNameChars()));
        }

        public bool includesNoWordleCharacters(string checkMe)
        {
            return checkMe.IndexOf('⬜') == -1 && checkMe.IndexOf('⬛') == -1 && checkMe.IndexOf("\U0001f7e8") == -1 && checkMe.IndexOf("\U0001f7e9") == -1 && checkMe.IndexOf("\U0001f7e6") == -1 && checkMe.IndexOf("\U0001f7e7") == -1 && checkMe.IndexOf("Cuadrado amarillo") == -1 && checkMe.IndexOf("Carré jaune") == -1 && checkMe.IndexOf("Cuadrado verde") == -1 && checkMe.IndexOf("Carré vert") == -1 && checkMe.IndexOf("Yellow square") == -1 && checkMe.IndexOf("Green square") == -1;
        }
    }
}