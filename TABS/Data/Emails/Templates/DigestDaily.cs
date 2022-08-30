namespace TABS.Data.Emails
{
    public class DigestDaily : ITemplate
    {
        public string GetSubject()
        {
            return "GSA - Condensé quotidien / ATM - Daily Digest";
        }

        public string GetTemplate()
        {
            return @"
<body style=""background - color: #fafafa;"">
  <span style=""padding: 24px 0 24px 0; font-family: helvetica;""><i>*English follows</i></span>
  <hr>
  <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" class=""wrapper"" style=""width: 100%;"">
    <tbody>
      <tr>
        <td style=""padding: 24px 0 24px 0;background-color: #fafafa"">
          <table width=""90%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""width: 90%;min-width: 300px;border: 1px solid #dddddd;background-color: #ffffff !important;margin: auto;"">
            <tbody>
              <tr>
                <td>
                  <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                    <tbody>
                      <tr>
                        <td class=""inner"" style=""color: #505050;font-size: 13px;padding: 24px;background-color: #ffffff;"">
                          <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                            <tbody>
                              <tr>
                                <td style=""background-color: #4F799F;font-size: 25px; padding: 10px; color: white;"">
                                  <span style=""font-family: helvetica; font-weight: 300;"">Mises à jour et rappels GSA</span>
                                </td>
                              </tr>
                              <tr>
                                <td style=""color: #505050; font-size: 16px; padding: 20px; background-color: #ffffff;"">
                                  <p style=""font-family: helvetica;"">
                                    Bonjour @Model.Name,
                                  </p>
                                  <p style=""font-family: helvetica;"">Voici vos mises à jour et vos rappels GSA pour @Model.StartDate.</p>
                                </td>
                              </tr>
                              <tr>
                                <td>
                                  @if (Model.Reminders[""fr-CA""].Count > 0) {
                                  <table width=""100%"" style=""background-color: #F5F5F5; padding: 10px 0px 10px 0px;"">
                                    <tbody>
                                      <tr>
                                        <td style=""color: #505050; font-size: 16px;"">
                                          <ul>
                                            <li>
                                              <h3 style=""font-family: helvetica;"">Rappels</h3>
                                              <ul>
                                                @foreach (object reminder in @Model.Reminders[""fr-CA""]) {
                                                <li style=""font-family: helvetica; margin: 10px 0px 10px 0px;""><a target=""_blank"" href=""https://@Model.Env/@reminder.GetType().GetProperty(""Link"").GetValue(reminder, null)"">@reminder.GetType().GetProperty(""AppName"").GetValue(reminder, null)</a> - @reminder.GetType().GetProperty(""Module"").GetValue(reminder, null) la prochaine mise à jour est requise le @reminder.GetType().GetProperty(""UpdateDate"").GetValue(reminder, null)</li>
                                                }
                                              </ul>
                                            </li>
                                          </ul>
                                        </td>
                                      </tr>
                                    </tbody>
                                  </table>
                                  <br>
                                  }
                                  @if (Model.Updates[""fr-CA""].Count > 0) {
                                  <table width=""100%"" style=""background-color: #F5F5F5; padding: 10px 0px 10px 0px;"">
                                    <tbody>
                                      <tr>
                                        <td style=""color: #505050; font-size: 16px;"">
                                          <ul>
                                            <li>
                                              <h3 style=""font-family: helvetica;"">Mises à jour</h3>
                                              <ul>
                                                @foreach (object update in Model.Updates[""fr-CA""]) {
                                                <li style=""font-family: helvetica; margin: 10px 0px 10px 0px;""><a target=""_blank"" href=""https://@Model.Env/@update.GetType().GetProperty(""Link"").GetValue(update, null)"">@update.GetType().GetProperty(""AppName"").GetValue(update, null)</a> - @update.GetType().GetProperty(""Module"").GetValue(update, null) a été mis à jour @update.GetType().GetProperty(""NumUpdates"").GetValue(update, null) fois</li>
                                                }
                                              </ul>
                                            </li>
                                          </ul>
                                        </td>
                                      </tr>
                                    </tbody>
                                  </table>
                                  }
                                </td>
                              </tr>
                              <tr>
                                <td style=""color: #505050; font-size: 16px; padding: 30px 0px 30px 20px;"">
                                  <a href=""https://@Model.Env/atm"" target=""_blank"" style=""padding: 10px; border: 2px solid #B3B3B3; text-decoration: none; color: #2B2B2B; font-family: helvetica;"">
                                    <b>Ouvrir GSA</b>
                                  </a>
                                </td>
                              </tr>
                              <tr>
                                <td style=""border-top: 1px solid #838383;"">
                                </td>
                              </tr>
                              <tr>
                                <td style=""padding: 20px; font-size: 16px; color: #707070; background-color: #ffffff;"">
                                  <a target=""_blank"" href=""https://@Model.Env/atm/settings?key=2"" style=""font-family: helvetica;"">Changer la fréquence des courriels</a><br><br>
                                  <a target=""_blank"" href=""https://@Model.Env/atm/settings?key=2"" style=""font-family: helvetica;"">Annuler votre inscription</a>
                                </td>
                              </tr>
                            </tbody>
                          </table>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </td>
              </tr>
            </tbody>
          </table>
        </td>
      <tr>
    </tbody>
  </table>
  <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" class=""wrapper"" style=""width: 100%;"">
    <tbody>
      <tr>
        <td style=""padding: 24px 0 24px 0;background-color: #fafafa"">
          <table width=""90%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""width: 90%;min-width: 300px;border: 1px solid #dddddd;background-color: #ffffff !important;margin: auto;"">
            <tbody>
              <tr>
                <td>
                  <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                    <tbody>
                      <tr>
                        <td class=""inner"" style=""color: #505050;font-size: 13px;padding: 24px;background-color: #ffffff;"">
                          <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"">
                            <tbody>
                              <tr>
                                <td style=""background-color: #4F799F;font-size: 25px; padding: 10px; color: white;"">
                                  <span style=""font-family: helvetica; font-weight: 300;"">ATM Updates and Reminders</span>
                                </td>
                              </tr>
                              <tr>
                                <td style=""color: #505050; font-size: 16px; padding: 20px; background-color: #ffffff;"">
                                  <p style=""font-family: helvetica;"">
                                    Hi @Model.Name,
                                  </p>
                                  <p style=""font-family: helvetica;"">Here are your updates and reminders from ATM for @Model.StartDate.</p>
                                </td>
                              </tr>
                              <tr>
                                <td>
                                  @if (Model.Reminders[""en-US""].Count > 0) {
                                  <table width=""100%"" style=""background-color: #F5F5F5; padding: 10px 0px 10px 0px;"">
                                    <tbody>
                                      <tr>
                                        <td style=""color: #505050; font-size: 16px;"">
                                          <ul>
                                            <li>
                                              <h3 style=""font-family: helvetica;"">Reminders</h3>
                                              <ul>
                                                @foreach (object reminder in @Model.Reminders[""en-US""]) {
                                                <li style=""font-family: helvetica; margin: 10px 0px 10px 0px;""><a target=""_blank"" href=""https://@Model.Env/@reminder.GetType().GetProperty(""Link"").GetValue(reminder, null)"">@reminder.GetType().GetProperty(""AppName"").GetValue(reminder, null)</a> - @reminder.GetType().GetProperty(""Module"").GetValue(reminder, null) next update is required on @reminder.GetType().GetProperty(""UpdateDate"").GetValue(reminder, null)</li>
                                                }
                                              </ul>
                                            </li>
                                          </ul>
                                        </td>
                                      </tr>
                                    </tbody>
                                  </table>
                                  <br>
                                  }
                                  @if (Model.Updates[""en-US""].Count > 0) {
                                  <table width=""100%"" style=""background-color: #F5F5F5; padding: 10px 0px 10px 0px;"">
                                    <tbody>
                                      <tr>
                                        <td style=""color: #505050; font-size: 16px;"">
                                          <ul>
                                            <li>
                                              <h3 style=""font-family: helvetica;"">Updates</h3>
                                              <ul>
                                                @foreach (object update in Model.Updates[""en-US""]) {
                                                <li style=""font-family: helvetica; margin: 10px 0px 10px 0px;""><a target=""_blank"" href=""https://@Model.Env/@update.GetType().GetProperty(""Link"").GetValue(update, null)"">@update.GetType().GetProperty(""AppName"").GetValue(update, null)</a> - @update.GetType().GetProperty(""Module"").GetValue(update, null) was updated @update.GetType().GetProperty(""NumUpdates"").GetValue(update, null) time(s)</li>
                                                }
                                              </ul>
                                            </li>
                                          </ul>
                                        </td>
                                      </tr>
                                    </tbody>
                                  </table>
                                  }
                                </td>
                              </tr>
                              <tr>
                                <td style=""color: #505050; font-size: 16px; padding: 30px 0px 30px 20px;"">
                                  <a href=""https://@Model.Env/atm"" target=""_blank"" style=""padding: 10px; border: 2px solid #B3B3B3; text-decoration: none; color: #2B2B2B; font-family: helvetica;"">
                                    <b>Open ATM</b>
                                  </a>
                                </td>
                              </tr>
                              <tr>
                                <td style=""border-top: 1px solid #838383;"">
                                </td>
                              </tr>
                              <tr>
                                <td style=""padding: 20px; font-size: 16px; color: #707070; background-color: #ffffff;"">
                                  <a target=""_blank"" href=""https://@Model.Env/atm/settings?key=2"" style=""font-family: helvetica;"">Change Email Frequency</a><br><br>
                                  <a target=""_blank"" href=""https://@Model.Env/atm/settings?key=2"" style=""font-family: helvetica;"">Unsubscribe</a>
                                </td>
                              </tr>
                            </tbody>
                          </table>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </td>
              </tr>
            </tbody>
          </table>
        </td>
      <tr>
    </tbody>
  </table>
</body>
";
        }
    }
}
