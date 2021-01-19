namespace VismaBugBountySelfServicePortal.Helpers.TokenAuth
{
   public interface ITokenSource
   {
        Token GetToken(bool refresh = false);
   }
}