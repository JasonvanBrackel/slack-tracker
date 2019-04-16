namespace Rancher.Community.Slack.Data.Models
{
    public partial class Users
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string Timezone { get; set; }
        public string TimezoneLabel { get; set; }
        public string ImagePath { get; set; }
    }
}
