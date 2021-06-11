using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Amazon_Transcribe_Speech_To_Text.Helpers.Models.AWServices
{
    public class AWSS3Service : AWSService
    {
        private AWSCredentials awsCredentials;
        private AmazonS3Client s3Client;
        private static readonly RegionEndpoint region = RegionEndpoint.USEast1;

        public AWSS3Service() {
            GetCredentialsAWS();
        }
        public bool GetCredentialsAWS()
        {
            CredentialProfileStoreChain credentialProfileChain = new CredentialProfileStoreChain();
            if (credentialProfileChain.TryGetAWSCredentials("AWS-Educate-profileD", out awsCredentials))
            {
                return true;
            }
            else
            {
                throw new ApplicationException("Erro obtendo credenciais");
            }
        }
        private bool S3Client()
        {
            if (GetCredentialsAWS())
            {
                s3Client = new AmazonS3Client(awsCredentials, region);
                return true;
            }
            return false;
        }

        public List<string> S3ListBuckets()
        {
            List<String> bucktes = new List<string>();
            if (S3Client())
            {
                ListBucketsResponse bucketsResponse = (s3Client.ListBucketsAsync()).Result;
                foreach (S3Bucket bucket in bucketsResponse.Buckets)
                {
                    bucktes.Add(bucket.BucketName);
                }
                return bucktes;
            }
            return null;
        }
        public bool checkFileOnBucket(AWSUtil awsUtilProperts)
        {
            List<string> audiosInBucket = audioInputBucketNames(awsUtilProperts);
            if (audiosInBucket.Count != 0)
            {
                bool check = audiosInBucket.Contains(Path.GetFileName(awsUtilProperts.NewFileName));
                if (check)
                {
                    awsUtilProperts.FileNameActual = awsUtilProperts.NewFileName;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        public async Task<bool> UploadAudioFromS3(AWSUtil awsUtilProperts)
        {
            try
            {
                if (S3Client())
                {
                    TransferUtility fileTransferUtility = new TransferUtility(s3Client);
                    await fileTransferUtility.UploadAsync(awsUtilProperts.FileNameActual, awsUtilProperts.BucketNameInput);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (AmazonS3Exception ex)
            {
                MessageBox.Show($"Erro encontrado no servidor, ao escrever objeto { ex.Message} ");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro encontrado no servidor, ao escrever objeto { ex.Message} ");
                return false;
            }

        }
        public async Task<string> UploadFileFromS3(byte[] content, string extension, AWSUtil awsUtilService)
        {
            try
            {
                if (S3Client())
                {
                    TransferUtility fileTransferUtility = new TransferUtility(s3Client);

                    string key = Guid.NewGuid().ToString();

                    TransferUtilityUploadRequest transferRequest = new TransferUtilityUploadRequest
                    {
                        Key = $"FilesTranslate-{key}/{key}.{extension}",
                        BucketName = awsUtilService.BucketNameInput,
                        ContentType = $"text/{extension}",
                        InputStream = new MemoryStream(content),
                    };

                    await fileTransferUtility.UploadAsync(transferRequest);
                    return "FilesTranslate-"+ key;
                }
                else
                {
                    return "";
                }
            }
            catch (AmazonS3Exception ex)
            {
                throw new ApplicationException($"Erro encontrado no servidor, ao escrever objeto { ex.Message} ");
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Erro encontrado no servidor, ao escrever objeto { ex.Message} ");
            }
        }

        public async Task<string> DownloadFileAsync(string fileRoute, AWSUtil awsUtilService)
        {
            try
            {
                string stringTranslate = "";
                if (S3Client())
                {
                    GetObjectRequest getObjectRequest = new GetObjectRequest
                    {
                        BucketName = awsUtilService.BucketNameOutput,
                        Key = fileRoute,
                    };
                    GetObjectResponse objectResponse = await s3Client.GetObjectAsync(getObjectRequest);
                    if (objectResponse.HttpStatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("O Arquivo não foi encontrado.");
                    }
                    MemoryStream result = new MemoryStream();
                    Stream responseFileStream = objectResponse.ResponseStream;
                    StreamReader reader = new StreamReader(responseFileStream);
                    responseFileStream.CopyTo(result);
                    stringTranslate = Encoding.UTF8.GetString(result.ToArray());

                }
                return stringTranslate;
            }
            catch (Exception)
            {
                throw new Exception("Ocorreu um erro ao obter o arquivo do Storage.");
            }
        }


        public List<string> audioInputBucketNames(AWSUtil awsUtilProperts)
        {
            awsUtilProperts.ExistingImagesBucket = new List<string>();
            AmazonS3Client amazonS3Client = new AmazonS3Client(awsCredentials, region);
            ListObjectsRequest request = new ListObjectsRequest
            {
                BucketName = awsUtilProperts.BucketNameInput
            };

            ListObjectsResponse listResponse = new ListObjectsResponse();
            do
            {
                listResponse = amazonS3Client.ListObjectsAsync(request).Result;
                foreach (S3Object s3Object in listResponse.S3Objects)
                {
                    awsUtilProperts.ExistingImagesBucket.Add(s3Object.Key);
                }
                request.Marker = listResponse.NextMarker;

            } while (listResponse.IsTruncated);

            return awsUtilProperts.ExistingImagesBucket;
        }

        public async Task<string> getObjectTranscribeS3(AWSUtil awsUtilProperts)
        {
            try
            {
                if (S3Client())
                {
                    GetObjectRequest getObjectRequest = new GetObjectRequest()
                    {
                        BucketName = awsUtilProperts.BucketNameOutput,
                        Key = awsUtilProperts.JobName
                    };
                    using (GetObjectResponse getObjectResponse = await s3Client.GetObjectAsync(getObjectRequest))
                    {
                        using (StreamReader reader = new StreamReader(getObjectResponse.ResponseStream))
                        {
                            string contents = reader.ReadToEnd();
                            return contents;
                        }
                    }
                }
                return String.Empty;
            }
            catch (WebException)
            {
                throw;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (AmazonS3Exception)
            {
                throw;
            }
        }

        public async Task<string> DownloadFileS3(AWSUtil awsUtilProperts)
        {
            try
            {
                if (S3Client())
                {
                    string nomeFile = @"..\..\..\Audios\" + awsUtilProperts.FileNameActual;
                    TransferUtility fileTransferUtility = new TransferUtility(s3Client);
                    TransferUtilityDownloadRequest transferUtilityDownloadRequest = new TransferUtilityDownloadRequest()
                    {
                        BucketName = awsUtilProperts.BucketNameInput,
                        Key = awsUtilProperts.FileNameActual,
                        FilePath = nomeFile,
                    };
                    await fileTransferUtility.DownloadAsync(transferUtilityDownloadRequest);
                    return nomeFile;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
