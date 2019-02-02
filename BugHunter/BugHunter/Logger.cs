using System;
using System.IO;

namespace ProjectWhitespace
{
    public class Logger
    {
        private string LogPath = null;
        
        public Logger(string LogPath)
        {
            this.LogPath = LogPath;
        }

        /// <summary>
        /// Loggt den übergeben String in das File das dem Constructor übergeben wurde
        /// </summary>
        /// <param name="message">Nachricht, welche geloggt werden sollte</param>
        /// <param name="tag">Optional: Tag für Nachricht. Standartmäßig "Info"</param>
        public void Log(string message, string tag = "Info")
        {
            StreamWriter swNew = null;
            StreamWriter swAppend = null;


            try
            {
                if (!File.Exists(this.LogPath))
                {
                    // Falls Logdatei nicht existiert wird eine neue erstellt
                    swNew = File.CreateText(this.LogPath);
                    swNew.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + tag + ":\t" + message);
                }
                else
                {
                    // Wenn Logdatei bereits vorhanden ist wird der aktuellle Log angehangen
                    swAppend = new StreamWriter(this.LogPath, true);
                    swAppend.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + tag + ":\t" + message);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
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
