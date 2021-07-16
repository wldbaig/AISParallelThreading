using AisUriProviderApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AisInterviewTask
{
    public interface IFileWrapper
    {
        bool Exists(string path);
        void Delete(string path);
    }

    public class FileWrapper : IFileWrapper
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public void Delete(string path)
        {
            File.Delete(path);
        }
    }

    public interface IDirectoryWrapper
    {
        bool Exists(string path);
        DirectoryInfo CreateDirectory(string path);
    }

    public class DirectoryWrapper : IDirectoryWrapper
    {
        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }
        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }
    }

    public class FileBusinessLogic : IDisposable
    {
        private readonly IFileWrapper fileWrapper;
        private readonly IDirectoryWrapper dirWrapper;
        private static readonly object _locker = new object();
        public FileBusinessLogic()
        {
            fileWrapper = new FileWrapper();
            dirWrapper = new DirectoryWrapper();
        }
        public FileBusinessLogic(IFileWrapper wrapper, IDirectoryWrapper dirwrapper)
        {
            this.fileWrapper = wrapper;
            this.dirWrapper = dirwrapper;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReadFilesFromLocalStorage()
        {
            string path = Directory.GetCurrentDirectory() + "/download/";
            var directory = new DirectoryInfo(path);
            if (directory != null)
            {
                var filePaths = directory.GetFiles().OrderByDescending(f => f.LastWriteTime)
                 .Take(10)
                 .ToArray();

                if (filePaths.Length == 0)
                {
                    throw new FileNotExistsInLocalStorageException("No File Exist in Local Storage");
                }
                else
                {
                    foreach (var file in filePaths)
                    {
                        Console.WriteLine("File : " + file.Name);
                    }
                }
            }
            else
            {
                throw new DirectoryNotFoundException("Directory not Found");
            }
        }

        /// <summary>
        /// Get Files From Uri, Convert IEnumerable<Uri> to List<Uri>
        /// </summary>
        /// <returns> List of Uri</returns>
        public List<Uri> GetFiles()
        {
            AisUriProvider obj = new AisUriProvider();
            List<Uri> files = new List<Uri>();
            try
            {
                files = obj.Get().ToList();
            }
            catch (Exception)
            {
                throw new FilesFromServerException("An error occur while fetching files from Uri Provider");
            }
            return files;
        }

        /// <summary>
        /// Download the files in a parallel way, having 3 parallel downloads at all times, 
        /// </summary>
        public void SyncFile()
        {
            var files = GetFiles(); // Getting files from Uri
            DisplayFetchedFilesFromServer(files);
            if (files.Count > 0)
            {
                if (!DirectoryExist("download"))
                {
                    CreateDirectory("download");
                }

                int maxDegreeOfParallelism = Convert.ToInt32(ConfigurationManager.AppSettings["MaxDegreeOfParallelism"].ToString());

                var tasks = Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
                 s =>
                {
                    DownloadFile(s);
                });
            }
            else
            {
                throw new FilesFromServerException("No files can be downloaded");
            }
        }

        /// <summary>
        /// Display files in console
        /// </summary>
        /// <param name="files"></param>
        private void DisplayFetchedFilesFromServer(List<Uri> files)
        {
            if (files.Count > 0)
            {
                Console.WriteLine("Current Files: ");
                foreach (var file in files)
                {
                    Console.WriteLine("file: " + Path.GetFileName(file.LocalPath));
                }
            }
        }

        public async Task DownloadFile(Uri uri)
        {
            lock (_locker)
            {
                try
                {
                    // path where download file to be saved, with filename, here I have taken file name from supplied remote url
                    string FilePath = Directory.GetCurrentDirectory() + "/download/" + Path.GetFileName(uri.LocalPath);

                    if (FileExist(Path.GetFileName(uri.LocalPath))) // check if file is present and not being used
                    {
                        DeleteFile(Path.GetFileName(uri.LocalPath)); // Remove old File
                    }

                    using (var client = new WebClient())
                    {
                        //delegate method, which will be called after file download has been complete.
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(Extract);
                        client.QueryString.Add("file", Path.GetFileName(uri.LocalPath)); // here you can add values
                                                                                         //delegate method for progress notification handler.
                                                                                         // client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgessChanged);
                                                                                         // uri is the remote url where filed needs to be downloaded, and FilePath is the location where file to be saved
                        client.DownloadFileTaskAsync(uri, FilePath);
                    }
                }
                catch (Exception)
                {
                    throw new FilesFromServerException("An error occured while downloading file");
                }
            }

        }
        public bool FileExist(string fileName)
        {
            string path = Directory.GetCurrentDirectory() + "/download/" + fileName;
            FileInfo fileInfo = new FileInfo(path);
            if (fileWrapper.Exists(path))
            {
                try
                {
                    using (FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        stream.Close();
                    }
                }
                catch (IOException)
                {
                    //the file is unavailable because it is:
                    //still being written to
                    //or being processed by another thread
                    //or does not exist (has already been processed)
                    Console.WriteLine(fileName + " is unavailable as its being used.");
                    return false;
                }

                return true;
            }

            //file is not locked
            return false;
        }

        public bool DirectoryExist(string directory)
        {
            return dirWrapper.Exists(directory);
        }
        public void CreateDirectory(string directory)
        {
            try
            {
                dirWrapper.CreateDirectory(directory);
            }
            catch (Exception)
            {
                throw new InvalidDirectoryException("An error occured while creating a directory");
            }
        }

        public void DeleteFile(string fileName)
        {
            try
            {
                fileWrapper.Delete(Directory.GetCurrentDirectory() + "/download/" + fileName);
                Console.WriteLine(fileName + " has been deleted");
            }
            catch (Exception)
            {
                throw new FileDeletingFromLocalStorageException("An error occured while deleting " + fileName);
            }
        }

        public static void Extract(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                throw new FilesFromServerException("File download cancelled.");
            }

            else if (e.Error != null)
            {
                throw new Exception(e.Error.ToString());
            }
            else
            {
                string fileIdentifier = ((System.Net.WebClient)(sender)).QueryString["file"];

                Console.WriteLine("File " + fileIdentifier + " has been downloaded.");
            }
        }

        public void ProgessChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string fileIdentifier = ((System.Net.WebClient)(sender)).QueryString["file"];

            // Displays the operation identifier, and the transfer progress.
            Console.WriteLine("{0} downloaded {1} of {2} bytes. {3} % complete...",
                 fileIdentifier,
                e.BytesReceived,
                e.TotalBytesToReceive,
                e.ProgressPercentage);
        }

        public void Dispose()
        {

        }
    }
}
