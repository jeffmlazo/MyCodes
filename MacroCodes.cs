				t.Cookies = o.cookies;
				string url = "http://www.yellowbook360.com/advertise?formVersion=freeListing";
				
				//***** BL Get All Category Text and Value*****//
				//Old code style
				string dir1 = @"c:\\CategText.txt";
                string dir2 = @"c:\\CategValue.txt";
                string text = null; string value = null;
                for (int icateg = 0; icateg < categOptions.Length; icateg++)
                {
                text += categOptions.Item(icateg).text + Environment.NewLine;
                value += categOptions.Item(icateg).value + Environment.NewLine;
                }

                File.WriteAllText(dir1, text);
                File.WriteAllText(dir2, value);
				
				//select gender
                switch (o.gender)
                {
                    case 1:
                        //female
                        FindElement("select", "name=users[gender]").Select("Female");
                        break;
                    default:
                        //male
                        FindElement("select", "name=users[gender]").Select("Male");
                        break;
                }
				
				//New code simple and style
				string dir1 = @"c:\\CategText.txt";
                string dir2 = @"c:\\CategValue.txt";
                File.Create(dir1).Close();
                File.Create(dir2).Close();
				for (int icateg = 0; icateg < categOptions.Length; icateg++)
                    {
                     File.AppendAllText(dir1, categOptions.Item(icateg).text + Environment.NewLine);
                     File.AppendAllText(dir2, categOptions.Item(icateg).value + Environment.NewLine);
                    }	
				//***** End BL Get All Category Text and Value*****//
				
				//***** Verification for o promotion Script*****//
				string[] errData = { o.title, o.url, o.content, o.tags };
                string[] errText = { "Title", "Url", "Content", "Tags" };
                for (int iText = 0; iText < errData.Length; iText++)
                {
                    if (String.IsNullOrEmpty(errData[iText]))
                    {
                        r.status = Constants.STATUS_ERROR;
                        r.error = errText[iText] + "is empty";
                        return;
                    }
                }
				//***** End Verification for o promotion Script*****//
				
                browser = t.Navigate(url);
                WaitToLoad();
				
				//***** BB Verification for blank and delay Script*****//
				IElement htmlTag;
				string[] delayContains = {"502 Bad Gateway",
										  "The server is currently experiencing a high load",
										  "Delicious will be down for scheduled maintenance"
										 };
				
				htmlTag = browser.FindElement("html");
                if (htmlTag != null)
                {
                    //check if the body text is empty
                    if (String.IsNullOrEmpty(htmlTag.text))
                    {
                        r.status = Constants.STATUS_DELAYED;
                        r.error = "Page does not load properly";
                        return;
                    }
                    else
                    {
                        //check here for server errors
                        for (int i = 0; i < delayContains.Length; i++)
                        {
                            if (htmlTag.text.Contains(delayContains[i]))
                            {
                                r.status = Constants.STATUS_DELAYED;
                                r.error = delayContains[i];
                                return;
                            }
                        }
                    }
                }
				//***** End BB Verification for blank and delay Script*****//

				//*****Start BB Split phone Script*****//
                string[] phone = o.phone.Split('-');
                FindElement("input", "name=phone_one").InputText(phone[0]);
                FindElement("input", "name=phone_two").InputText(phone[1]);
                FindElement("input", "name=phone_three").InputText(phone[2]);
				//*****End BB Split phone Script*****//
				
			    //*****Start BL Split phone Script*****//
                string[] phone = o.phone.Split('-');
                browser.AddFormField("areacode", phone[0]);
                browser.AddFormField("exchange", phone[1]);
			    browser.AddFormField("phone", phone[2]);
				//*****End BL Split phone Script*****//
				
				
				//***** Fields Script*****//
                browser.referer = url;
                FindElement("input", "name=FirstName").InputText(o.firstName);
				browser.AddFormField("InterestedAdvertising.Directory", "false");
				
                //***** Get State Script*****//
                ////so this is the alternative code for State
                ElementList options = browser.FindAllElements("option");
                string stateValue = null;
                for (int iOptions = 0; iOptions < options.Length; iOptions++)
                {
                    string optionsText = options.Item(iOptions).text;
                   if (options.Item(iOptions).text.Trim().Contains(o.state))
                   {
                       stateValue = options.Item(iOptions).value;
                       break;
                   }
                }

				//***** Logged in Verification Script*****//
                bool login = true;
                if (FindElement("a", "id=header-signOutLink") != null || FindElement("a", "text=Sign Out") != null)
                {
                    if (FindElement("a", "text=" + o.userName) != null || browser.sourceCode.Contains(o.userName))
                    {
                        login = false;
                    }
					else
					{
					    browser = t.Navigate(url);
						WaitToLoad();
					}
				}
				
                if (login)
                {
                    Element myYbMsg = FindElement("li", "class=myYbMessage");
                    if (myYbMsg != null && myYbMsg.text.Contains("but your email and/or password is"))
                    {
                        r.status = Constants.STATUS_ERROR;
                        r.error = Constants.INVALID_LOGIN;
                        return;
                    }
                    else if (myYbMsg != null && myYbMsg.text.Contains("Your account has not been activated"))
                    {
                        r.status = Constants.STATUS_PENDING_ACTIVATION;
                        return;
                    }
                }

				//***** RegEx Script*****//
                string gidValue = null;
                Match regexMatch = Regex.Match(browser.sourceCode, "(req=addev&gid.+)");
                if (regexMatch.Success && regexMatch.Groups.Count == 2)
                {
                    string[] gidValSplit = regexMatch.Groups[0].Value.Split(new string[] { "gid=", QUOTE }, StringSplitOptions.RemoveEmptyEntries);
                    gidValue = gidValSplit[1].Trim();
                }
                else
                {
                    r.status = Constants.STATUS_ERROR;
                    r.error = "ERROR: req=addev&gid  - failed to get value";
                    return;
                }
				
                browser.Navigate("http://www.cityslick.net/reg.php?req=addev&gid=" + gidValue);
                WaitToLoad();

				//***** BB Script Random Element*****//
				IElement ulSuggestedReddits = FindElement("div", "id=suggested-reddits").FindChildElement("ul");
                IElementList liChoices = ulSuggestedReddits.FindChildrenElements("li");
                Random rndNum = new Random();
                liChoices[rndNum.Next(1, liChoices.length)].FindChildElement("a").Click();
			    
				
				//***** Birhtday Script*****//
				string[] bDay = o.birthday.Split('-');
                string birth_Month, birth_Day, birth_Year;

                if (o.birthday != null) // birthday is not empty or null 
                {
                    string[] bday_array = o.birthday.Split('-');
                    birth_Year = bday_array[0].Trim();
                    birth_Month = bday_array[1].TrimStart('0');
                    birth_Day = bday_array[2].TrimStart('0');
                }
                else // if none or null or other input - Set a default value "January 1, 1989"
                {
                    birth_Month = "1";
                    birth_Day = "1";
                    birth_Year = "1989";
                }

				//***** Month Script for date numbers to Text *****//
                DateTime now = DateTime.Now;
                string month = null;
                for (int iMonth = 0; iMonth < 12; iMonth++)
                {
                    //convert the value for Months ex. 11 to Nov
                    if (now.Month.ToString().Equals(birth_Month))
                    {
                        month = now.ToString("MMM");
                        break;
                    }
                    now = now.AddMonths(1);
                }
				
				//***** Start Description has a maximum of 200 characters only *****//
                string newDesc = null;
                if (desc1.Length > 200)
                {
                    newDesc = Shorten(desc1, 200);
                }
                else
                {
                    newDesc = desc1;
                }
                browser.AddFormField("business", newDesc); //Description
                //*Note: Remaining length for Description
                int remLenDesc = 200 - newDesc.Length;
                browser.AddFormField("remLen", remLenDesc.ToString());
				//***** End Description has a maximum of 200 characters only *****//
				
				//***** Fixed and Random Category Script*****//
				string categVal = null;
				if (String.IsNullOrEmpty(o.category) || o.category == "none")
                {
                    //Default random category for empty or none categories
                    try
                    {
                        string[] categNames = { "articlecategory", "category" };
                        ElementList categOptions = null;
                        for (int iCategNames = 0; iCategNames < categNames.Length; iCategNames++)
                        {
                            if (FindElement("select", "name=" + categNames[iCategNames]) != null)
                            {
                                categOptions = FindElement("select", "name=" + categNames[iCategNames]).FindAllElements("option");
                                break;
                            }
                        }

                        Random rndNum = new Random();
                        categVal = categOptions.Item(rndNum.Next(0, categOptions.Length)).value;
                    }
                    catch
                    {
                        r.status = Constants.STATUS_ERROR;
                        r.error = "There is a problem in choosing random category";
                        return;
                    }
                }
                else
                {
                    try
                    {
                        categVal = browser.FindElement("option", "text=" + "*" + o.category + "*").value;
                    }
                    catch
                    {
                        r.status = Constants.STATUS_ERROR;
                        r.error = "Category in option did not match";
                        return;
                    }
                }
				
				//***** BB RECAPTCHA Script*****//
                for (int retry = 0; true; )
                {
                    browser.core.useRegExp = true;
                    IElement captcha = FindElement("img", "src=https://www.google.com/recaptcha/api/image?.*");
                    browser.core.useRegExp = false;

                    string captchaPath = t.GetCaptchaPath("delicious");
                    captcha.SaveElementImage(captchaPath);
                    string captchaText = t.SolveCaptcha(captchaPath, o.automationObjectId);

                    FindElement("input text", "id=recaptcha_response_field").InputText(captchaText);

                    if (captchaText.ToLower().Equals("x"))
                    {
                        r.status = Constants.STATUS_DELAYED;
                        r.error = "No Captcha Solution";
                        return;
                    }

                    IElement noRobotsText = FindElement("a", "text=No Robots Here");
                    IElement noRobotsClass = FindElement("a", "class=btn green registerSubmit");

                    if (noRobotsText != null)
                    {
                        noRobotsText.Click();
                    }
                    else
                    {
                        noRobotsClass.Click();
                    }
                    browser.WaitToLoad();


                    Thread.Sleep(2000);
                    IElement checkCaptcha = FindElement("input text", "id=recaptcha_response_field");

                    if (checkCaptcha == null)
                    {
                        t.LogCaptchaResult((long)o.automationObjectId, Path.GetFileName(captchaPath), Constants.CAPTCHA_RESULT_RESOLVED, null);
                        break;
                    }

                    t.LogCaptchaResult((long)o.automationObjectId, Path.GetFileName(captchaPath), Constants.CAPTCHA_RESULT_NO_MATCH, null);

                    if (++retry == 3)
                    {
                        r.status = Constants.STATUS_DELAYED;
                        r.error = Constants.CAPTCHA_RESULT_NO_MATCH;
                        return;
                    }
                }
				
				//***** BL RECAPTCHA Script*****//
				Recaptcha recaptcha = new Recaptcha(browser);
                if (recaptcha.Initialized)
                {
                    for (int retry = 0; true; )
                    {
                        string captchaPath = t.GetCaptchaPath("cityslick");
                        if (recaptcha.SaveElementImage(captchaPath))
                        {
                            string captchaText = t.SolveCaptcha(captchaPath, o.automationObjectId);
                            if (recaptcha.SetChallengeCode(captchaText))
                            {
                                t.LogCaptchaResult(o.automationObjectId, Path.GetFileName(captchaPath), Constants.CAPTCHA_RESULT_RESOLVED, null);
                                break;
                            }
                            else
                            {
                                t.LogCaptchaResult((long)o.automationObjectId, Path.GetFileName(captchaPath), Constants.CAPTCHA_RESULT_NO_MATCH, null);
                                if (++retry == 3)
                                {
                                    r.status = Constants.STATUS_DELAYED;
                                    r.error = Constants.CAPTCHA_RESULT_NO_MATCH;
                                    return;
                                }
                                recaptcha.TryAnotherChallenge();
                            }
                        }
                    }
                }
                else
                {
                    r.status = Constants.STATUS_ERROR;
                    r.error = "Recaptcha widget could not be initialized.";
                    return;
                }
				
				//***** BL CAPTCHA Script*****//
				Element captchaCheck = FindElement("div", "text=Confirm you're human");
                if (captchaCheck != null)
                {
                    for (int retry = 0; true; )
                    {
                        FindElement("input", "name=email").InputText(o.emailAddress);
                        FindElement("input", "name=email2").InputText(o.emailAddress);
                        string go_cVal = FindElement("input hidden", "name=go_c").value;
                        browser.AddFormField("go_c", go_cVal);

                        Element captcha = FindElement("img", "src=*/captchaimage.php*");
                        string captchaPath = t.GetCaptchaPath("brownbook");
                        captcha.SaveElementImage(captchaPath);
                        string captchaText = t.SolveCaptcha(captchaPath, o.automationObjectId);

                        if (captchaText.ToLower().Equals("x") || captchaText.ToLower().Equals("0"))
                        {
                            r.status = Constants.STATUS_DELAYED;
                            r.error = "No captcha solution";
                            return;
                        }

                        FindElement("input", "name=confirm").InputText(captchaText);
                        browser.AddFormField("submit.x", "58");
                        browser.AddFormField("submit.y", "4");
                        browser.AddFormField("submit", "<< Go >>");
                        string goVal = FindElement("input hidden", "name=go").value;
                        browser.AddFormField("go", goVal);
                        browser.Post(url);
                        WaitToLoad();

                        Element captchaError = FindElement("div", "text=The characters you've entered don't match those in the image");
                        if (captchaError == null)
                        {
                            t.LogCaptchaResult((long)o.automationObjectId, Path.GetFileName(captchaPath), Constants.CAPTCHA_RESULT_RESOLVED, null);
                            break;
                        }
                        else
                        {
                            t.LogCaptchaResult((long)o.automationObjectId, Path.GetFileName(captchaPath), Constants.CAPTCHA_RESULT_NO_MATCH, null);
                            if (++retry == 3)
                            {
                                r.status = Constants.STATUS_DELAYED;
                                r.error = Constants.CAPTCHA_RESULT_NO_MATCH;
                                return;
                            }
                        }
                    }
                }
				
				 //***** Start BB Get All options tags and text Script*****//
                string url = "http://articledirectorydatabase.com/";
                string path = @"c:\\getCategories.txt";

                File.Create(path).Close();
                string host = new Uri(url).Host.Replace("www.", string.Empty);
                browser = t.NavigateClearCookies(url, host, 2500);
                browser.core.loadTimeoutIsError = false;
                browser.WaitToLoad();

                FindElement("a","text=Login").Click();
                WaitToLoad();

                FindElement("input", "name=uname").InputText(o.emailAddress);
                FindElement("input", "name=pswd").InputText(o.accountPassword);
                FindElement("input submit", "value=Login").Click();
                WaitToLoad();

                FindElement("a","text=Submit Articles").Click();
                WaitToLoad();

                // Change attribute of the 2nd param
                IElement selectElement = FindElement("select", "id=parentId");
                if (selectElement != null)
                {
                    IElementList allOptions = selectElement.FindAllElements("option");
                    if (allOptions != null && allOptions.length != 0)
                    {
                        for (int i = 0; i < allOptions.length; i++)
                        {
                            // Change format if not the same with the original source
                            File.AppendAllText(path, "<option value=\"" + allOptions[i].GetAttribute("value") + "\">" + allOptions[i].text + "</option>" + LF);
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("ERROR: Options not found");
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("ERROR: Select element not found");
                }

                System.Windows.Forms.MessageBox.Show("COMPLETE!");
                //***** End BB Get All options tags and text Script*****//
				
				//*****Start Script for debugging in gmail and stats*****//
				 throw new Exception("debug instance - can't find verification for success msg in sending password reset");
				 throw new Exception("debug instance - account verification failed");
				//*****End Script for debugging in gmail and stats*****//