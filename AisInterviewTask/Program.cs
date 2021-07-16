using System;
using System.Configuration;
using System.Threading;  

namespace AisInterviewTask
{
    public class Program
    { 
        public static void Main(string[] args)
        {

            try
            { 
                Console.WriteLine("===========Welcome=============");
                Console.WriteLine("Getting Filenames from local storage");
                Console.WriteLine("===============================");
                using (FileBusinessLogic obj = new FileBusinessLogic())
                {
                    obj.ReadFilesFromLocalStorage();
                } 
            } 
            catch(FileNotExistsInLocalStorageException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
            }
           
            catch (InvalidDirectoryException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (FileDeletingFromLocalStorageException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            try
            {
                Console.WriteLine("===============================");
                Console.WriteLine("Start Downloading new files: ");
                ScheduleService();
            }
            catch (FilesFromServerException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (FileDeletingFromLocalStorageException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }
         
        public static void ScheduleService()
        { 
            try
            {
                var intervalSeconds = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalSeconds"].ToString());
                Timer T = new Timer(new TimerCallback(SchedularCallback), null, 0, intervalSeconds); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); 
            }
        }

        private static void SchedularCallback(object state)
        { 
            using (FileBusinessLogic obj = new FileBusinessLogic())
            {
                obj.SyncFile();
            }
        }
    }
}
