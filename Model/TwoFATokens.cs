namespace CoachOnline.Model
{
    public class TwoFATokens
    {
        public int Id { get; set; }
        public bool Deactivated { get; set; }
        public string Token { get; set; }
        public long ValidateTo { get; set; }
        public TwoFaTokensTypes Type { get; set; }
        public string AdditionalInfo { get; set; }
    }

    public enum TwoFaTokensTypes { UNKNOWN, RESET_PASSWORD, EMAIL_CONFIRMATION, DATA_CONFIRMATION, EMAIL_CHANGE_CONFIRMATION }
}