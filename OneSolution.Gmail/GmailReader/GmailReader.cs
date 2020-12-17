using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneSolution.Gmail
{
    public class GmailReader
    {
        public async Task GetEmails(string emailAddr = "gcq888@gmail.com", string label = "ARK/ArkTrading")
        {
            try
            {
                UserCredential credential;
                using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        // This OAuth 2.0 access scope allows for read-only access to the authenticated 
                        // user's account, but not other types of account access.
                        new[] { GmailService.Scope.GmailReadonly },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(this.GetType().ToString())
                    );
                }

                var gmailService = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = this.GetType().ToString()
                });

                var emailListRequest = gmailService.Users.Messages.List(emailAddr);
                //emailListRequest.LabelIds = label;
                emailListRequest.IncludeSpamTrash = false;
                //emailListRequest.Q = "is:unread"; // This was added because I only wanted unread emails...

                // Get our emails
                var emailListResponse = await emailListRequest.ExecuteAsync();

                if (emailListResponse != null && emailListResponse.Messages != null)
                {
                    // Loop through each email and get what fields you want...
                    foreach (var email in emailListResponse.Messages)
                    {
                        var emailInfoRequest = gmailService.Users.Messages.Get(emailAddr, email.Id);
                        // Make another request for that email id...
                        var emailInfoResponse = await emailInfoRequest.ExecuteAsync();

                        if (emailInfoResponse != null)
                        {
                            String from = "";
                            String date = "";
                            String subject = "";
                            String body = "";
                            // Loop through the headers and get the fields we need...
                            foreach (var mParts in emailInfoResponse.Payload.Headers)
                            {
                                if (mParts.Name == "Date")
                                {
                                    date = mParts.Value;
                                }
                                else if (mParts.Name == "From")
                                {
                                    from = mParts.Value;
                                }
                                else if (mParts.Name == "Subject")
                                {
                                    subject = mParts.Value;
                                }

                                if (date != "" && from != "")
                                {
                                    if (emailInfoResponse.Payload.Parts == null && emailInfoResponse.Payload.Body != null)
                                    {
                                        body = emailInfoResponse.Payload.Body.Data;
                                    }
                                    else
                                    {
                                        body = getNestedParts(emailInfoResponse.Payload.Parts, "");
                                    }
                                    // Need to replace some characters as the data for the email's body is base64
                                    String codedBody = body.Replace("-", "+");
                                    codedBody = codedBody.Replace("_", "/");
                                    byte[] data = Convert.FromBase64String(codedBody);
                                    body = Encoding.UTF8.GetString(data);

                                    // Now you have the data you want...                         
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Failed to get messages!", "Failed Messages!", MessageBoxButtons.OK);
            }
        }

        static String getNestedParts(IList<MessagePart> part, string curr)
        {
            string str = curr;
            if (part == null)
            {
                return str;
            }
            else
            {
                foreach (var parts in part)
                {
                    if (parts.Parts == null)
                    {
                        if (parts.Body != null && parts.Body.Data != null)
                        {
                            str += parts.Body.Data;
                        }
                    }
                    else
                    {
                        return getNestedParts(parts.Parts, str);
                    }
                }

                return str;
            }
        }
    }
}
