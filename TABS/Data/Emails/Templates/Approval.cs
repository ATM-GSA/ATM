namespace TABS.Data.Emails
{
    public class Approval : ITemplate
    {

        public string GetSubject()
        {
            return "GSA - Inscription approuvée / ATM - Registration Approved";
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
                                  <span style=""font-family: helvetica; font-weight: 300;"">Compte GSA approuvé</span>
                                </td>
                              </tr>
                              <tr>
                                <td style=""color: #505050; font-size: 16px; padding: 20px; background-color: #ffffff;"">
                                  <p style=""font-family: helvetica;"">
                                    Bonjour @Model.Name,
                                  </p>
                                  <p style=""font-family: helvetica;"">Votre compte GSA a été approuvé, vous pouvez maintenant accéder à l’application.</p>
                                </td>
                              </tr>
                              <tr>
                                <td>
                                  <table width=""100%"" style=""background-color: #F5F5F5;"">
                                    <tbody>
                                      <tr>
                                        <td style=""color: #505050; font-size: 16px; padding: 10px 0px 10px 20px;"">
                                          @if (Model.Role == 1) {
                                          <h4 style=""font-family: helvetica;"">Rôle assigné : Lecture seule</h4>
                                          } else if (Model.Role == 2) {
                                          <h4 style=""font-family: helvetica;"">Rôle assigné : Utilisateur avancé</h4>
                                          } else if (Model.Role == 3) {
                                          <h4 style=""font-family: helvetica;"">Rôle assigné : Administration</h4>
                                          }
                                        </td>
                                      </tr>
                                    </tbody>
                                  </table>
                                </td>
                              </tr>
                              <tr>
                                <td style=""color: #505050; font-size: 16px; padding: 20px 0px 20px 20px;"">
                                  <a href=""https://@Model.Env/atm"" target=""_blank"" style=""padding: 10px; border: 2px solid #B3B3B3; text-decoration: none; color: #2B2B2B; font-family: helvetica;"">
                                    <b>Ouvrir GSA</b>
                                  </a>
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
                                  <span style=""font-family: helvetica; font-weight: 300;"">ATM Account Approved</span>
                                </td>
                              </tr>
                              <tr>
                                <td style=""color: #505050; font-size: 16px; padding: 20px; background-color: #ffffff;"">
                                  <p style=""font-family: helvetica;"">
                                    Hello @Model.Name,
                                  </p>
                                  <p style=""font-family: helvetica;"">Your ATM account has been approved, you can now access the application.</p>
                                </td>
                              </tr>
                              <tr>
                                <td>
                                  <table width=""100%"" style=""background-color: #F5F5F5;"">
                                    <tbody>
                                      <tr>
                                        <td style=""color: #505050; font-size: 16px; padding: 10px 0px 10px 20px;"">
                                          @if (Model.Role == 1) {
                                          <h4 style=""font-family: helvetica;"">Assigned Role: Read Only</h4>
                                          } else if (Model.Role == 2) {
                                          <h4 style=""font-family: helvetica;"">Assigned Role: Power User</h4>
                                          } else if (Model.Role == 3) {
                                          <h4 style=""font-family: helvetica;"">Assigned Role: Admin</h4>
                                          }
                                        </td>
                                      </tr>
                                    </tbody>
                                  </table>
                                </td>
                              </tr>
                              <tr>
                                <td style=""color: #505050; font-size: 16px; padding: 20px 0px 20px 20px;"">
                                  <a href=""https://@Model.Env/atm"" target=""_blank"" style=""padding: 10px; border: 2px solid #B3B3B3; text-decoration: none; color: #2B2B2B; font-family: helvetica;"">
                                    <b>Open ATM</b>
                                  </a>
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
