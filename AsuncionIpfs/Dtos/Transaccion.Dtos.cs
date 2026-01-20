namespace AsuncionIpfs.Dtos
{
    public class TxMetadataDto
    {
        public string txHash { get; set; } = "";
     public string network { get; set; } = "";
        public List<TxMetadataLabelDto> labels { get; set; } = new();
    }

    public class TxMetadataLabelDto
    {
     public string label { get; set; } = "";
     public object? json { get; set; }
    }
}
