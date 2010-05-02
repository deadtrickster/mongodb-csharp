using System;
using System.IO;
using MongoDB.Driver.Protocol;
using NUnit.Framework;
using MongoDB.Driver.IO;

namespace MongoDB.Driver.Connections
{
    [TestFixture()]
    public class TestConnection
    {       
        [Test]
        public void TestSendQueryMessage(){
            //Connection conn = new Connection("10.141.153.2");
            Connections.Connection conn = ConnectionFactory.GetConnection(string.Empty);
            conn.Open();
            
            QueryMessage qmsg = generateQueryMessage();
            conn.SendTwoWayMessage(qmsg);
            
            conn.Close();
        }
        
        [Test]
        public void TestReconnectOnce(){
            Connections.Connection conn = ConnectionFactory.GetConnection(string.Empty);
            conn.Open();
                        
            WriteBadMessage(conn);
            try{    
                QueryMessage qmsg = generateQueryMessage();
                conn.SendTwoWayMessage(qmsg);
                
            }catch(IOException){
                //Should be able to resend.
                Assert.IsTrue(conn.State == ConnectionState.Opened);
                QueryMessage qmsg = generateQueryMessage();
                ReplyMessage rmsg = conn.SendTwoWayMessage(qmsg);
                Assert.IsNotNull(rmsg);
            
            }
        }
        
        protected void WriteBadMessage(Connections.Connection conn){
            //Write a bad message to the socket to force mongo to shut down our connection.
            BinaryWriter writer = new BinaryWriter(conn.GetStream());
            System.Text.UTF8Encoding  encoding=new System.Text.UTF8Encoding(); 
            Byte[] msg = encoding.GetBytes("Goodbye MongoDB!");
            writer.Write(16 + msg.Length + 1);
            writer.Write(1);
            writer.Write(1);
            writer.Write(1001);
            writer.Write(msg);
            writer.Write((byte)0);
        }
        
        protected QueryMessage generateQueryMessage(){
            Document qdoc = new Document();
            qdoc.Add("listDatabases", 1.0);
            //QueryMessage qmsg = new QueryMessage(qdoc,"system.namespaces");
            QueryMessage qmsg = new QueryMessage(qdoc,"admin.$cmd");
            qmsg.NumberToReturn = -1;
            
            return qmsg;
        }
    }
}