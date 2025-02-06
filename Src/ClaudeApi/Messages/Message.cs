using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClaudeApi.Messages
{

    [JsonConverter(typeof(MessageConverter))]
    public class Message : INotifyPropertyChanged
    {
        private string? _role;
        private ObservableCollection<ContentBlock>? _content = [];

        [JsonProperty("role")]
        public string? Role
        {
            get => _role;
            set
            {
                if (_role != value)
                {
                    _role = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty("content")]
        public ObservableCollection<ContentBlock>? Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged();
                }
            }
        }

        public void AppendContent(ContentBlock contentBlock)
        {
            _content ??= [];
            _content.Add(contentBlock);
            OnPropertyChanged(nameof(Content));
        }

        public override string ToString()
        {
            return $"{Role}: {string.Join("", Content?.Select(cb => cb.ToString()) ?? [])}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MessageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Message);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            var message = new Message
            {
                Role = (string?)jo["role"]
            };

            JToken? contentToken = jo["content"];
            if (contentToken?.Type == JTokenType.String)
            {
                message.Content = [ContentBlock.FromString(contentToken.ToString())];
            }
            else if (contentToken?.Type == JTokenType.Array)
            {
                message.Content = contentToken.ToObject<ObservableCollection<ContentBlock>>(serializer);
            }
            else
            {
                throw new JsonSerializationException("Unexpected content type");
            }

            return message;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var message = (Message?)value;
            var jo = new JObject
            {
                ["role"] = message?.Role
            };

            if (message?.Content is not null)
            {
                jo["content"] = JToken.FromObject(message.Content, serializer);
            }
            else
            {
                throw new JsonSerializationException("Unexpected content type");
            }

            jo.WriteTo(writer);
        }
    }
}
