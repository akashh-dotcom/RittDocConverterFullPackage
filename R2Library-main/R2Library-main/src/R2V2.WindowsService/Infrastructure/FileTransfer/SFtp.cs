#region

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using Common.Logging;
using Tamir.SharpSsh;
using Tamir.SharpSsh.jsch;
using Tamir.Streams;

#endregion

namespace R2V2.WindowsService.Infrastructure.FileTransfer
{
    public class SFtp
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public string Hostname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public bool SendFile(string localFullFilePath, string destinationFileName)
        {
            SshTransferProtocolBase sshCp = null;
            try
            {
                sshCp = new Sftp(Hostname, Username, Password);
                sshCp.Connect();
                sshCp.Put(localFullFilePath, destinationFileName);
                return true;
            }
            catch (Exception ex)
            {
                var errorMsg = new StringBuilder()
                    .AppendFormat("Hostname: {0}, Username: {1}, localFullFilePath: {2}, destinationFileName: {3}",
                        Hostname, Username, localFullFilePath, destinationFileName)
                    .AppendLine().AppendFormat("Message: {0}", ex.Message);
                Log.Error(errorMsg.ToString(), ex);
                return false;
            }
            finally
            {
                if (sshCp != null)
                {
                    sshCp.Close();
                }
            }
        }

        public bool WriteStringAsFile(string fileContent, string destinationFileName)
        {
            try
            {
                var jsch = new JSch();
                var session = jsch.getSession(Username, Hostname, 22);
                Log.DebugFormat("Username: {0}, Hostname: {1}", Username, Hostname);
                session.setPassword(Password);
                Log.DebugFormat("Password: {0}", Password);

                var config = new Hashtable { { "StrictHostKeyChecking", "no" } };
                session.setConfig(config);
                Log.Debug("configs set");

                session.connect();
                Log.Debug("connection established");
                var channel = session.openChannel("sftp");
                Log.Debug("channel opended");
                channel.connect();
                Log.Debug("channel connected");
                var channelSftp = (ChannelSftp)channel;

                var asciiEncoding = new ASCIIEncoding();
                var writeString = asciiEncoding.GetBytes(fileContent);
                Log.DebugFormat("writeString.Length: {0}", writeString.Length);
                var memoryStream = new MemoryStream(writeString.Length);
                Log.Debug("attempting to write stream ...");
                memoryStream.Write(writeString, 0, writeString.Length);
                Log.Debug("stream written");
                memoryStream.Seek(0, SeekOrigin.Begin);

                Log.Debug("creating input stream wrapper");
                var inputStream = new InputStreamWrapper(memoryStream);

                Log.DebugFormat("attempting put, destinationFileName: {0}", destinationFileName);
                channelSftp.put(inputStream, destinationFileName);
                Log.Debug("transfer complete");
                return true;
            }
            catch (Exception ex)
            {
                var errorMsg = new StringBuilder()
                    .AppendFormat("Hostname: {0}, Username: {1}, destinationFileName: {2}", Hostname, Username,
                        destinationFileName)
                    .AppendLine().AppendFormat("Message: {0}", ex.Message);
                Log.Error(errorMsg.ToString(), ex);
                ErrorMessage = $"sFTP Error - {ex.Message}";
                return false;
            }
        }

        public bool DoesFileExistOnRemoteServer(string filename)
        {
            Sftp sftp = null;
            try
            {
                sftp = new Sftp(Hostname, Username, Password);

                sftp.Connect();

                //the foldername cannot be empty, or the listing will not show
                var res = sftp.GetFileList("/");
                foreach (var item in res)
                {
                    Log.DebugFormat("file: {0}", item);
                    if (filename == item.ToString())
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                var errorMsg = new StringBuilder()
                    .AppendFormat("Hostname: {0}, Username: {1}, filename: {2}",
                        Hostname, Username, filename)
                    .AppendLine().AppendFormat("Message: {0}", ex.Message);
                Log.Error(errorMsg.ToString(), ex);
                return false;
            }
            finally
            {
                if (sftp != null)
                {
                    sftp.Close();
                }
            }
        }
    }
}