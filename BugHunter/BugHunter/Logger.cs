using BugHunter;
using System;
using System.Collections.Generic;
using System.IO;

namespace ProjectWhitespace
{
    public class Logger
    {
        private string LogPath = null;
        public static List<String> LogQueue = new List<string>();

        public Logger(string LogPath)
        {
            this.LogPath = LogPath;
        }

        /// <summary>
        /// Fügt Parameter zur Log-Warteschlange hinzu
        /// </summary>
        /// <param name="message">Lognachricht</param>
        /// <param name="tag">Tag für Kategorisierung der Log-Nachricht</param>
        public void Log(string message, string source = "", string tag = "Info")
        {
            LogQueue.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + "\tTag: " + tag + "\t Source: " + source +  "\tMessage:\t" + message);
        }

        /// <summary>
        /// Schreibt die Lognachrichten in der Log-Warteschleife gesammelt raus
        /// </summary>
        public void WriteLog()
        {
            StreamWriter swNew = null;
            StreamWriter swAppend = null;

            try
            {
                if (!File.Exists(this.LogPath))
                {
                    // Falls Logdatei nicht existiert wird eine neue erstellt
                    swNew = File.CreateText(this.LogPath);
                    
                    // Schreibt alle Logs in der LogQueue in das File
                    foreach(string Log in LogQueue)
                    {
                        swNew.WriteLineAsync(Log);
                    }
                    LogQueue.Clear();   // Löscht alle Einträge in der LogQueue da diese Eingetragen wurden
                }
                else
                {
                    // Wenn Logdatei bereits vorhanden ist wird der aktuellle Log angehangen
                    swAppend = new StreamWriter(this.LogPath, true);

                    // Schreibt alle Logs in der LogQueue in das File
                    foreach (string Log in LogQueue)
                    {
                        swAppend.WriteLineAsync(Log);
                    }
                    LogQueue.Clear();   // Löscht alle Einträge in der LogQueue da diese Eingetragen wurden
                }
            }
            catch(Exception e)
            {
                string tag = "Error";
                string source = "WriteLog";
                string message = e.Message;

                LogQueue.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + "\tTag: " + tag + "\t Source: " + source + "\tMessage:\t" + message);
            }
            finally
            {
                if (swNew != null)
                    swNew.Close();
                if (swAppend != null)
                    swAppend.Close();
            }
        }
    }
}
