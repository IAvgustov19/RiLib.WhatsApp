using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ydb.Sdk;
using Ydb.Sdk.Services.Table;
using Ydb.Sdk.Yc;
using Ydb.Sdk.Value;

//VERSION 1.0  26.10.23
namespace RiLib.WhatsApp
{
    public class Main
    {
        private static int IdInstance;
        private static string ApiTokenInstance;

        public static string DefaultLinuxPath = "";
        public static string DefaulWinPath = "C:\\Users\\avgus\\OneDrive\\Рабочий стол\\";

        public static TableClient? tableClient { get; set; }

        //Перенести данные в библиотеку
        public static void SetProfileData(int IdInstance, string ApiTokenInstance)
        {
            Main.IdInstance = IdInstance;
            Main.ApiTokenInstance = ApiTokenInstance;
        }
        public async static void InitializationYDB(string endpoint, string database, string keypath)
        {
            var saProvider = new ServiceAccountProvider(
  saFilePath: keypath);

            var config = new DriverConfig(
    endpoint: endpoint,
    database: database,
    credentials: saProvider
);

            using var driver = new Driver(
                config: config
            );

            await driver.Initialize(); // Make sure to await driver initialization

            // Create Ydb.Sdk.Table.TableClient using Driver instance.
            tableClient = new TableClient(driver, new TableClientConfig());
        }
        public async static Task<IReadOnlyList<ResultSet>> SendRequestToYDB(string query, Dictionary<string, YdbValue> dictionary = null)
        {
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, YdbValue>();
            }

            var response = await tableClient.SessionExec(async session =>
            {
                return await session.ExecuteDataQuery(
                    query: query,
                    parameters: dictionary,
                    txControl: TxControl.BeginSerializableRW().Commit()
                );
            });

            response.Status.EnsureSuccess();

            var queryResponse = (ExecuteDataQueryResponse)response;
            return queryResponse.Result.ResultSets;
        }

        public async static Task SendMessageRequest(string? text, string number)
        {
            await new HttpClient().PostAsync($"https://api.green-api.com/waInstance{IdInstance}/sendMessage/{ApiTokenInstance}",
                                               new StringContent(JsonConvert.SerializeObject(new SendMessege { chatId = number, message = text }), Encoding.UTF8,
                                               "application/json"));
            await Task.Delay(100);
        }

        public async static Task SendMessageUrlRequest(string text, string fileName, string url, string number, string loadtext = "Пожалуйста подождите... Загружаю...")
        {
            await SendMessageRequest(loadtext, number);
            await new HttpClient().PostAsync($"https://api.green-api.com/waInstance{IdInstance}/sendFileByUrl/{ApiTokenInstance}\r\n",
                                               new StringContent(JsonConvert.SerializeObject(new SendUrlMessege
                                               {
                                                   chatId = number,
                                                   caption = text,
                                                   fileName = fileName,
                                                   urlFile = url
                                               }), Encoding.UTF8,
                                               "application/json"));
            await Task.Delay(100);
        }

        public async static Task<string> ReceiveNotification()
        {
            try
            {
                var Response = await new HttpClient().GetAsync($"https://api.green-api.com/waInstance{IdInstance}/receiveNotification/{ApiTokenInstance}");
                return await Response?.Content.ReadAsStringAsync();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
    public class StructGettingNotification
    {
        public long receiptId { get; set; }
        public Body body { get; set; }
    }
    public class Body
    {
        public string typeWebhook { get; set; }
        public SenderData senderData { get; set; }
        public MessageData messageData { get; set; }
    }
    public class SenderData
    {
        public string chatId { get; set; }
        public string sender { get; set; }
    }
    public class MessageData
    {
        public string typeMessage { get; set; }
        public TextMessageData textMessageData { get; set; }
        public ExtendedTextMessageData extendedTextMessageData { get; set; }
        public QuotedMessageData quotedMessage { get; set; }
    }
    public class TextMessageData
    {
        public string? textMessage { get; set; }
    }
    public class ExtendedTextMessageData
    {
        public string? text { get; set; }
    }
    public class QuotedMessageData
    {
        public string? caption { get; set; }
    }

    public class SendMessege
    {
        public string chatId { get; set; }
        public string message { get; set; }
        public bool linkPreview { get; set; }
    }
    public class SendUrlMessege
    {
        public string chatId { get; set; }
        public string caption { get; set; }
        public string fileName { get; set; }
        public string urlFile { get; set; }
    }
    public class CreateGroupRequest
    {
        public string groupName { get; set; }
        public string[] chatIds { get; set; }
    }
    public class CreateGroupResponse
    {
        public bool created { get; set; }
        public string chatId { get; set; }
        public string groupInviteLink { get; set; }
    }
}