using Microsoft.WindowsAzure.Storage.Table;

namespace Library.Njuskalo
{
    class PersistedEntity : TableEntity
    {
        public string Url { get; set; }
        public bool Notified { get; set; }
    }
}
