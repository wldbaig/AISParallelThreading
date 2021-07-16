using System.Runtime.Serialization;

namespace AisInterviewTask
{
    public class FileNotExistsInLocalStorageException : System.Exception
    {
        public FileNotExistsInLocalStorageException() : base() { }
        public FileNotExistsInLocalStorageException(string message) : base(message) { }
        public FileNotExistsInLocalStorageException(string message, System.Exception innerException) : base(message, innerException) { }
        protected FileNotExistsInLocalStorageException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class FileDeletingFromLocalStorageException : System.Exception
    {
        public FileDeletingFromLocalStorageException() : base() { }
        public FileDeletingFromLocalStorageException(string message) : base(message) { }
        public FileDeletingFromLocalStorageException(string message, System.Exception innerException) : base(message, innerException) { }
        protected FileDeletingFromLocalStorageException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class DirectoryNotFoundException : System.Exception
    {
        public DirectoryNotFoundException() : base() { }
        public DirectoryNotFoundException(string message) : base(message) { }
        public DirectoryNotFoundException(string message, System.Exception innerException) : base(message, innerException) { }
        protected DirectoryNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class InvalidDirectoryException : System.Exception
    {
        public InvalidDirectoryException() : base() { }
        public InvalidDirectoryException(string message) : base(message) { }
        public InvalidDirectoryException(string message, System.Exception innerException) : base(message, innerException) { }
        protected InvalidDirectoryException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class FilesFromServerException : System.Exception
    {
        public FilesFromServerException() : base() { }
        public FilesFromServerException(string message) : base(message) { }
        public FilesFromServerException(string message, System.Exception innerException) : base(message, innerException) { }
        protected FilesFromServerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
