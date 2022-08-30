namespace TABS.Data.Emails
{
    public class Alert : ITemplate
    {
        public string GetSubject()
        {
            return "GSA - Rappel de la mise à jour du module / ATM - Module Update Reminder";
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
                                  <span style=""font-family: helvetica; font-weight: 300;"">Rappel GSA</span>
                                </td>
                              </tr>
                              <tr>
                                <td style=""color: #505050; font-size: 16px; padding: 20px; background-color: #ffffff;"">
                                  <p style=""font-family: helvetica;"">
                                    Bonjour @Model.Name,
                                  </p>
                                  <p style=""font-family: helvetica;"">Il y a certains modules dont la prochaine mise à jour est requise le <b>@Model.UpdateDate</b> (dans une semaine).</p>
                                </td>
                              </tr>
                              <tr>
                                <td>
                                  <table width=""100%"" style=""background-color: #F5F5F5; padding: 5px 0px 5px 0px;"">
                                        <tbody>
                                          <tr>
                                            <td style=""color: #505050; font-size: 16px;"">
                                              <ul>
                                                @foreach (object reminder in @Model.Reminders[""fr-CA""]) {
                                                    <li style=""font-family: helvetica; margin: 10px 0px 10px 0px"">
                                                      <a target=""_blank"" href=""https://@Model.Env/@reminder.GetType().GetProperty(""Link"").GetValue(reminder, null)""><b>@reminder.GetType().GetProperty(""AppName"").GetValue(reminder, null)</b></a> - @reminder.GetType().GetProperty(""Module"").GetValue(reminder, null)
                                                    </li>
                                                }
                                              </ul>
                                            </td>
                                          </tr>
                                        </tbody>
                                      </table>
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
                                  <span style=""font-family: helvetica; font-weight: 300;"">ATM Reminder</span>
                                </td>
                              </tr>
                              <tr>
                                <td style=""color: #505050; font-size: 16px; padding: 20px; background-color: #ffffff;"">
                                  <p style=""font-family: helvetica;"">
                                    Hi @Model.Name,
                                  </p>
                                  <p style=""font-family: helvetica;"">There are some modules whose next update is required on <b>@Model.UpdateDate</b> (in one week).</p>
                                </td>
                              </tr>
                              <tr>
                                <td>
                                  <table width=""100%"" style=""background-color: #F5F5F5; padding: 5px 0px 5px 0px;"">
                                        <tbody>
                                          <tr>
                                            <td style=""color: #505050; font-size: 16px;"">
                                              <ul>
                                                @foreach (object reminder in @Model.Reminders[""en-US""]) {
                                                    <li style=""font-family: helvetica; margin: 10px 0px 10px 0px"">
                                                      <a target=""_blank"" href=""https://@Model.Env/@reminder.GetType().GetProperty(""Link"").GetValue(reminder, null)""><b>@reminder.GetType().GetProperty(""AppName"").GetValue(reminder, null)</b></a> - @reminder.GetType().GetProperty(""Module"").GetValue(reminder, null)
                                                    </li>
                                                }
                                              </ul>
                                            </td>
                                          </tr>
                                        </tbody>
                                      </table>
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
