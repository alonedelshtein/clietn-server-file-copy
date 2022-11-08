
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Program
{
    const int kb = 1024;
    const string fileName = "test";
    const string recieveMode = "r";
    const string uploadMode = "u";
    const string ackMessage = "-@-";
    const string doneMessage = "@-@";
    const int bytesCountToSendMessage = 100*kb;
    static readonly string fileSource = Path.Combine("/Users/alonedelshtein/Desktop/source",fileName);//Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),fileName);    
    static readonly string fileDestination = Path.Combine("/Users/alonedelshtein/Desktop/dest",fileName);//Path.Combine(Path.GetTempPath(),fileName);
    


    public static void Main(string[] args)
    {
        Console.WriteLine("Hello");        
        Console.WriteLine("I am a file copy program using socket and stream");        
        Console.WriteLine("I can run in both 'upload' mode or 'recieve' mode");                
        Console.WriteLine("Please chose your mode:");              
        Console.WriteLine("Please chose your mode ('r' for 'recieve' or 'u' for 'upload'):");
        
        bool gotCommand = false;
        while(!gotCommand)
        {            
            var line = Console.ReadLine();
            switch(line)
            {
                case recieveMode:
                    listen();
                    gotCommand=true;
                    break;
                case uploadMode:
                    upload();
                    gotCommand=true;
                    break;
                default:
                    Console.WriteLine("Sorry, I did not undersatnd");                    
                    Console.WriteLine("Please try again");
                    Console.WriteLine("Please chose your mode ('r' for 'recieve' or 'u' for 'upload'):");
                    break;

            }
        }
    }

    private static void listen()
    {
        try
        {
            Console.WriteLine("Try to listen");
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 43522));

            server.Listen(1);

            Console.WriteLine("Server start listen on 127.0.0.1:43522...");
            while(true)
            {
                Socket client = server.Accept();

                Console.WriteLine("Server accepted client");
                
                Console.WriteLine("Server start writting file");
            
                    
                using(StreamWriter sw = new StreamWriter(fileDestination,false))
                {
                    int recivedBytes = 0;
                    int totalRecievedBytes = 0;
                    byte[] buffer = new byte[kb];
                    int countBytesToMessage = 0;
                    while(true)
                    {
                        recivedBytes = client.Receive(buffer,SocketFlags.None);
                        if(recivedBytes == 0)
                            break;
                        totalRecievedBytes += recivedBytes;
                        countBytesToMessage += recivedBytes;
                        var recievedChars = Encoding.ASCII.GetChars(buffer,0,recivedBytes);
                        
                        if(recievedChars.Length==doneMessage.Length)
                        {
                            bool isDone = true;
                            for(int i=0;i < 3;i++)
                            {
                                if(recievedChars[i] != doneMessage[i])
                                {
                                    isDone = false;
                                    break;
                                }
                            }
                            if (isDone)                                
                                break;
                        }

                        sw.Write(recievedChars,0,kb);

                        if(countBytesToMessage > bytesCountToSendMessage)
                        {
                            countBytesToMessage = 0;
                            Console.WriteLine($"Server wrote {totalRecievedBytes} total bytes so far");
                        }
                        client.Send(Encoding.ASCII.GetBytes(ackMessage), SocketFlags.None);
                    }

                    sw.Close();
                    Console.WriteLine($"Server completed successfully to write {fileDestination}");
                    
                    Console.WriteLine("########--####--########");
                    Console.WriteLine("########^^####^^########");
                    Console.WriteLine("###########@@###########");
                    Console.WriteLine("###########@@###########");
                    Console.WriteLine("#####^^##########^^#####");
                    Console.WriteLine("#######^^######^^#######");
                    Console.WriteLine("#########^^^^^^#########");
                    Console.WriteLine("Let's try again ;-)");
                    Console.WriteLine("Server start listen on 127.0.0.1:43522...");

                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

    }
    
    private static void upload()
    {
        try
        {
            
            Console.WriteLine("Try to connect");
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 43522));
            
            Console.WriteLine("Client is connected to the server on 127.0.0.1:43522");
            
            using(FileStream fs = new FileStream(fileSource, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    int readBytes = 0;
                    int totalReadBytes = 0;
                    int sentBytes = 0;
                    int totalSentBytes = 0;
                    byte[] buffer = new byte[kb];
                    int countReadBytesToMessage = 0;
                    int countSentBytesToMessage = 0;
                    while((readBytes = sr.BaseStream.Read(buffer,0,kb)) > 0)
                    {
                        totalReadBytes += readBytes;
                        countReadBytesToMessage += readBytes;
                        if(countReadBytesToMessage > bytesCountToSendMessage)
                        {
                            countReadBytesToMessage = 0;
                            Console.WriteLine($"Client read {totalReadBytes} total bytes so far");
                        }

                        sentBytes = client.Send(buffer,SocketFlags.None);

                        totalSentBytes += sentBytes;       
                        countSentBytesToMessage += sentBytes;             
                        if(countSentBytesToMessage > bytesCountToSendMessage)
                        {
                            countSentBytesToMessage = 0;
                            Console.WriteLine($"Client read {totalSentBytes} total bytes so far");
                        }
                        readBytes = client.Receive(buffer,SocketFlags.None);
                        var recievedString = Encoding.ASCII.GetString(buffer,0,readBytes);
                        
                        if(string.Compare(recievedString,ackMessage) != 0)
                        {
                            Console.WriteLine($"Server did not ack, exist program");                            
                            sr.Close();
                            break;
                        }
                    }
                    sr.Close();

                    Console.WriteLine($"Client completed successfully to send {fileSource}");
                    
                    Console.WriteLine("########--####--########");
                    Console.WriteLine("########^^####^^########");
                    Console.WriteLine("###########@@###########");
                    Console.WriteLine("###########@@###########");
                    Console.WriteLine("#####^^##########^^#####");
                    Console.WriteLine("#######^^######^^#######");
                    Console.WriteLine("#########^^^^^^#########");
                    Console.WriteLine("Feel free to run and try client again ;-)");                    
                }
            }
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

}

