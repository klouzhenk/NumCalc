# NumCalc

## Email (password reset)

Password reset emails are sent via Gmail SMTP from a dedicated project inbox.
SMTP credentials live in user-secrets on `NumCalc.User.API` (`EmailSettings:Smtp:*`).

**Note:** the first emails from a fresh sender often land in **Spam**. Check the
spam folder if the reset link doesn't appear in the inbox within a minute.
