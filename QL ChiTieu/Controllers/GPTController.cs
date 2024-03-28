using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI_API.Completions;
using OpenAI_API;
using Newtonsoft.Json;
using QL_ChiTieu.Models;
using System.Text;

namespace QL_ChiTieu.Controllers
{ 
    [ApiController]
    public class GPTController : ControllerBase
    {
        [HttpPost]
        [Route("AskChatGPT")]
        public async Task<IActionResult> AskChatGPT([FromBody] string query)
        {
            string chatURL = "https://api.openai.com/v1/chat/completions";
            string apiKey = "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";
            StringBuilder sb = new StringBuilder();

            HttpClient oClient = new HttpClient();
            oClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            ChatRequest oRequest = new ChatRequest();
            //oRequest.model = "gpt-4-1106-preview";
            //Dùng chatGPT 4 thì dòng trên
            oRequest.model = "gpt-3.5-turbo-1106";

            Message oMessage = new Message();
            oMessage.role = "user";
            oMessage.content = query;

            oRequest.messages = new Message[] { oMessage };

            string oReqJSON = JsonConvert.SerializeObject(oRequest);
            HttpContent oContent = new StringContent(oReqJSON, Encoding.UTF8, "application/json");

            HttpResponseMessage oResponseMessage = await oClient.PostAsync(chatURL, oContent);

            if (oResponseMessage.IsSuccessStatusCode)
            {
                string oResJSON = await oResponseMessage.Content.ReadAsStringAsync();

                ChatResponse oResponse = JsonConvert.DeserializeObject<ChatResponse>(oResJSON);

                foreach (Models.Choice c in oResponse.choices)
                {
                    sb.Append(c.message.content);
                }

                HttpChatGPTResponse oHttpResponse = new HttpChatGPTResponse()
                {
                    Success = true,
                    Data = sb.ToString()
                };

                return Ok(oHttpResponse);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
