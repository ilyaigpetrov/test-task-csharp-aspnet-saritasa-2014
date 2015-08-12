using System;

namespace SaritasaShortestPath
{
    /*
        An exception which is safe to present to
        a client.
    */
    public class ClientSafeException : System.Exception
    {
        public override string Message
        {
            get
            {
                return MessagePie;
            }
        }
        private string MessagePie;

        public ClientSafeException() : base()
        {
        }
        public ClientSafeException(string message)
        {
            MessagePie = message;
        }
        public ClientSafeException(string message, Exception innerExceptionToFilter)
        {
            this.MessagePie = message;
            var safeEx = innerExceptionToFilter as ClientSafeException;
            if (safeEx != null)
                this.MessagePie += "\r\n" + safeEx.Message;
        }
        public void AugmentMessageWithLine(string message)
        {
            this.MessagePie += "\r\n" + message;
        }
    }

    public class FileReadingSafeException : ClientSafeException
    {
        public FileReadingSafeException(string message, Exception exceptionToFilter) : base(message, exceptionToFilter)
        { }
    }

    public class inputValidationSafeException : ClientSafeException
    {
        public inputValidationSafeException(string message, Exception exceptionToFilter) : base(message, exceptionToFilter)
        { }
    }
}
