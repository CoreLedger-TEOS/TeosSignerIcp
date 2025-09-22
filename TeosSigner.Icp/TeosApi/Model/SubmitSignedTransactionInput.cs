namespace TeosSigner.Icp.TeosApi.Model;

class SubmitSignedTransactionInput
{
	public string SignerAddress { get; set; }
	public string SignedTransaction { get; set; }
	public string Description { get; set; }
}
