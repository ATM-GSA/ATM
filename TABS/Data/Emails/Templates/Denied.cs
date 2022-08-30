namespace TABS.Data.Emails
{
    public class Denied : ITemplate
    {
        public string GetSubject()
        {
            return "GSA - Inscription refusée / ATM - Registration Denied";
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
                                  <span style=""font-family: helvetica; font-weight: 300;"">Compte GSA refusé</span>
                                </td>
                              </tr>
                              <tr>
                                <td style=""color: #505050; font-size: 16px; padding: 20px; background-color: #ffffff;"">
                                  <p style=""font-family: helvetica;"">
                                    Bonjour @Model.Name,
                                  </p>
                                  <p style=""font-family: helvetica; padding: 10px 0px 10px 0px"">
                                    Votre demande de création de compte GSA a été refusée.<br><br>
                                    Pour toute question concernant cette décision, veuillez contacter votre gestionnaire.
                                  </p>
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
                                  <span style=""font-family: helvetica; font-weight: 300;"">ATM Account Denied</span>
                                </td>
                              </tr>
                              <tr>
                                <td style=""color: #505050; font-size: 16px; padding: 20px; background-color: #ffffff;"">
                                  <p style=""font-family: helvetica;"">
                                    Hello @Model.Name,
                                  </p>
                                  <p style=""font-family: helvetica; padding: 10px 0px 10px 0px"">
                                    Your ATM account registration request has been denied.<br><br>
                                    For questions regarding this decision, please contact your manager.
                                  </p>
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

