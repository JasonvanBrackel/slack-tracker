using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Web;
using Rancher.Community.Slack.Data;
using Rancher.Community.Slack.Models;
using Rancher.Community.Slack.Utilities;

namespace Rancher.Community.Slack.SlackApi
{
    public class ApiClient
    {
        private readonly string _url;
        private readonly string _authorizationToken;
        private HttpClient _client;
        private UriBuilder _uriBuilder;

        public ApiClient(string url, string authorizationToken)
        {
            _url = url;
            _authorizationToken = authorizationToken;
            ResetQueryParameters();
            _client = new HttpClient();
        }

        private void ResetQueryParameters()
        {
            _uriBuilder = new UriBuilder
            {
                Scheme = "https",
                Host = _url
            };
            var query = HttpUtility.ParseQueryString(_uriBuilder.Query);
            query["token"] = _authorizationToken;
            _uriBuilder.Query = query.ToString();
        }

        public MembersListResponse GetUsers(string cursor = null)
        {
            ResetQueryParameters();
            var query = HttpUtility.ParseQueryString(_uriBuilder.Query);
            query["token"] = _authorizationToken;
            if (cursor != null)
            {
                query["cursor"] = cursor;
            }
            _uriBuilder.Query = query.ToString();
            const string path = "api/users.list";
            _uriBuilder.Path = path;
            var response = _client.GetAsync(_uriBuilder.Uri).Result;
            return response.ReadFromJson<MembersListResponse>();
        }

        public UserInfoResponse GetUser(string Id)
        {
            ResetQueryParameters();
            var query = HttpUtility.ParseQueryString(_uriBuilder.Query);
            query["token"] = _authorizationToken;
            query["user"] = Id;
            _uriBuilder.Query = query.ToString();
    
            const string path = "api/users.info";
            _uriBuilder.Path = path;
            var response = _client.GetAsync(_uriBuilder.Uri).Result;
            return response.ReadFromJson<UserInfoResponse>();
        }

        public ChannelListResponse GetChannels()
        {
            const string path = "api/channels.list";
            _uriBuilder.Path = path;
            return _client.GetAsync(_uriBuilder.Uri).Result.ReadFromJson<ChannelListResponse>();

        }

        public ConversationHistoryResponse GetMessageHistory(string channelId, DateTime oldest, DateTime latest, string cursor = null)
        {
            ResetQueryParameters();
            var query = HttpUtility.ParseQueryString(_uriBuilder.Query);
            query["token"] = _authorizationToken;
            query["channel"] = channelId;
            query["oldest"] = oldest.ToSlackTimestamp();
            query["latest"] = latest.ToSlackTimestamp();
            query["inclusive"] = "true";
            if (cursor != null)
            {
                query["cursor"] = cursor;
            }
            
            
            _uriBuilder.Query = query.ToString();
    
            const string path = "api/conversations.history";
            _uriBuilder.Path = path;
            var response = _client.GetAsync(_uriBuilder.Uri).Result;
            Console.WriteLine($"Debug: { response.ReadAsString() }");
            return response.ReadFromJson<ConversationHistoryResponse>();
        }

        public void PostMessage(string channel, string message)
        {
            ResetQueryParameters();
            var query = HttpUtility.ParseQueryString(_uriBuilder.Query);
            query["token"] = _authorizationToken;
            query["channel"] = channel;
            query["text"] = message;
            query["as_user"] = "true";
            
            
            _uriBuilder.Query = query.ToString();
    
            const string path = "api/chat.postMessage";
            _uriBuilder.Path = path;
            var response = _client.GetAsync(_uriBuilder.Uri).Result;
        }

        public bool IsActive(string welcomerId)
        {
            ResetQueryParameters();
            var query = HttpUtility.ParseQueryString(_uriBuilder.Query);
            query["token"] = _authorizationToken;
            query["user"] = welcomerId;


            _uriBuilder.Query = query.ToString();

            const string path = "api/users.getPresence";
            _uriBuilder.Path = path;
            var response = _client.GetAsync(_uriBuilder.Uri).Result;

            var presence = response.ReadFromJson<GetPresenceReponse>();

            return presence.presence.Equals("active");
        }
    }
}