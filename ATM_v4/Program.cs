using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ATM_v4
{
    public class ATM : Form
    {
        // creating button and image variables
        Button[,] numBtns = new Button[3, 4];
        Button btnEnter = new Button();
        Button btnCancel = new Button();
        Button btnClear = new Button();
        TextBox txtDisplay = new TextBox();
        PictureBox moneyGif, cardGif, loadingGif, cardPic;

        private static Account[] ac = new Account[3];
        Account activeAccount;
        static Semaphore semaphore = new Semaphore(1, 1);
        static Semaphore fileSemaphore = new Semaphore(1, 1);
        private Account[] blockedCards = new Account[2];

        static int thrd = 1;
        bool accountNumberValidated = false;
        bool loginMenu = false;
        bool withdrawMenu = false;
        bool loading;
        string numbersEntered = "";
        string accountNumber = "";
        string pinNumber = "";
        private bool exit = false;
        private int pinCounter = 0;
        static string path = @"logInfo.txt";
        public static bool raceCondition;
        public static int numOfATMs;
        


        /*
         * ATM constructor
         */
        public ATM()
        {
            exit = false;
            initialiseForm();
            enterAccountNumber();
        }


        /*
         * Method to create the form
         */
        public void initialiseForm()
        {
            for (int x = 0; x < 3; x++) //Create buttons 
            {
                for (int y = 0; y < 4; y++)
                {
                    numBtns[x, y] = new Button();
                    numBtns[x, y].Name = Convert.ToString((x + 1) + "," + (y + 1));
                    numBtns[x, y].SetBounds((80 * x) + 150, (80 * y) + 300, 70, 70);
                    numBtns[x, y].BackColor = Color.AliceBlue;
                    numBtns[x, y].ForeColor = Color.DarkSlateGray;
                    numBtns[x, y].Font = new Font("Envy Code", 14, FontStyle.Bold);
                    numBtns[x, y].Click += new EventHandler(this.NumBtnEvent_Click);
                    Controls.Add(numBtns[x, y]);

                    if (y == 0)
                    {
                        numBtns[x, y].Text = Convert.ToString(x + 1);
                    }
                    else if (y == 1)
                    {
                        numBtns[x, y].Text = Convert.ToString(x + 4);
                    }
                    else if (y == 2)
                    {
                        numBtns[x, y].Text = Convert.ToString(x + 7);
                    }
                }
            }

            numBtns[1, 3].Text = "0";
            
            //Create gifs
            cardGif = new PictureBox();
            cardGif.Image = Image.FromFile(@"cardGif.GIF");
            cardGif.SetBounds(550, 320, 239, 360);
            Controls.Add(cardGif);
            cardGif.Visible = false;

            cardPic = new PictureBox();
            cardPic.Image = Image.FromFile(@"cardImage.png");
            cardPic.SetBounds(550, 320, 239, 360);
            Controls.Add(cardPic);
            cardPic.Visible = false;

            moneyGif = new PictureBox();
            moneyGif.Image = Image.FromFile(@"moneyGif.GIF");
            moneyGif.Location = new Point(300, 700);
            moneyGif.SetBounds(180, 650, 350, 150);
            moneyGif.SizeMode = PictureBoxSizeMode.StretchImage;
            Controls.Add(moneyGif);
            moneyGif.Visible = false;

            loadingGif = new PictureBox();
            loadingGif.Image = Image.FromFile(@"loadingEclipse.GIF");
            loadingGif.SetBounds(330, 120, 100, 100);
            loadingGif.BackColor = Color.DarkSlateGray;
            Controls.Add(loadingGif);
            loadingGif.Visible = false;

            TextBox moneyBox = new TextBox();
            moneyBox.BorderStyle = BorderStyle.FixedSingle;
            moneyBox.Multiline = true;
            moneyBox.BackColor = Color.LightSlateGray;
            moneyBox.SetBounds(180, 650, 350, 150);
            Controls.Add(moneyBox);

            TextBox cardBox = new TextBox();
            cardBox.BorderStyle = BorderStyle.FixedSingle;
            cardBox.Multiline = true;
            cardBox.BackColor = Color.LightSlateGray;
            cardBox.SetBounds(550, 320, 200, 20);
            Controls.Add(cardBox);

            //Create enter, clear and cancel buttons
            btnEnter.SetBounds(420, 300, 100, 70);
            btnEnter.BackColor = Color.ForestGreen;
            btnEnter.Text = "Enter";
            btnEnter.Click += new EventHandler(this.BtnEnterEvent_Click);
            Controls.Add(btnEnter);

            btnClear.SetBounds(420, 380, 100, 70);
            btnClear.BackColor = Color.Goldenrod;
            btnClear.Text = "Clear";
            btnClear.Click += new EventHandler(this.BtnClearEvent_Click);
            Controls.Add(btnClear);

            btnCancel.SetBounds(420, 460, 100, 70);
            btnCancel.BackColor = Color.DarkRed;
            btnCancel.Text = "Cancel";
            btnCancel.Click += new EventHandler(this.BtnCancelEvent_Click);
            Controls.Add(btnCancel);

            numBtns[0, 3].Visible = false; //Hide button
            numBtns[2, 3].Visible = false; //Hide button

            //create text box
            txtDisplay.Multiline = true;
            txtDisplay.Width = 500;
            txtDisplay.Height = 200;
            txtDisplay.Location = new Point(100, 50);
            txtDisplay.Font = new Font("Envy code", 12, FontStyle.Bold);
            txtDisplay.ForeColor = Color.AliceBlue;
            txtDisplay.BackColor = Color.DarkSlateGray;
            Controls.Add(txtDisplay);
        }


        /*
         * Event handler for numbered buttons
         */
        private void NumBtnEvent_Click(object sender, EventArgs e)
        {
            // if the card gif is off (not loading) then able to type
            if (loading == false)
            {
                numbersEntered += (((Button)sender).Text);
                // hides PIN number being typed in
                if (accountNumberValidated == true && loginMenu == false) //If user is entering pin number
                {
                    txtDisplay.Text += "*"; //Hide pin
                }
                else
                {
                    txtDisplay.Text += (((Button)sender).Text);
                }
            }  
        }


        /*
         * Event handler for enter button
         */
        async void BtnEnterEvent_Click(object sender, EventArgs e)
        {
            if (withdrawMenu == false) //User has not selected withdraw money
            {
                if (loginMenu == false) //  User has not logged in
                {
                    if (accountNumberValidated == false) // user has not entered a valid account number
                    {
                        accountNumber = numbersEntered;
                        numbersEntered = null;
                        if (accountNumber.Length != 6) //Check if the account number is 6 digits
                        {
                            txtDisplay.Text = "";
                            txtDisplay.Text = ("Please make sure your account number is 6 digits\r\n");
                            enterAccountNumber(); //prompt for account number
                        }
                        else //Account number is the right length
                        {
                            cardGif.Visible = true; //Show card going in
                            loading = true;
                            loadingGif.Visible = true;
                            await Task.Delay(3500);
                            cardGif.Visible = false;
                            loadingGif.Visible = false;
                            loading = false;
                            activeAccount = this.findAccount();
                            if (activeAccount != null)
                            {
                                accountNumberValidated = true;
                                for (int i = 0; i < 2; i++)
                                {
                                    if (blockedCards[i] == activeAccount)
                                    {
                                        txtDisplay.Text = ("Sorry, your card has been blocked. Please talk to your bank to reactivate.");
                                        await Task.Delay(2000);
                                        accountNumberValidated = false;
                                        loginMenu = false;
                                        withdrawMenu = false;
                                        numbersEntered = "";
                                        accountNumber = "";
                                        pinNumber = "";
                                        txtDisplay.Text = "";
                                        pinCounter = 0;
                                        enterAccountNumber();
                                        return;
                                    }
                                }
                                enterPinNumber(); //Prompt for pin
                            }
                            else //Invalid account number
                            {
                                MessageBox.Show("Account number not recognised", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txtDisplay.Text = ("");
                                enterAccountNumber();
                            }
                        }
                    }
                    else //User has entered a valid account number
                    {
                        pinNumber = numbersEntered;
                        numbersEntered = null;
                        if (pinNumber.Length != 4) //Check length of pin
                        {
                            txtDisplay.Text = "";
                            txtDisplay.Text = ("Please make sure your pin number is 4 digits\r\n");
                            enterPinNumber();
                        }
                        else //Pin is correct length
                        {
                            bool valid = activeAccount.checkPin(Convert.ToInt32(pinNumber));
                            if (valid == true) //Pin is valid
                            {
                                numbersEntered = null;
                                loginMenu = true;
                                displayLoginMenu();
                            }
                            else //Invalid pin
                            {
                                MessageBox.Show("Incorrect pin", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txtDisplay.Text = ("");
                                pinCounter++;
                                txtDisplay.Text = ("Number of tries: " + pinCounter);
                                if (pinCounter >= 3)
                                {
                                    txtDisplay.Text = ("Incorrect pin entered 3 times - blocking card");
                                    if (blockedCards[0] == null) //Adding account to blocked account array
                                    {
                                        blockedCards[0] = activeAccount;
                                    }
                                    else if (blockedCards[1] == null)
                                    {
                                        blockedCards[1] = activeAccount;
                                    }
                                    else
                                    {
                                        blockedCards[2] = activeAccount;
                                    }
                                    await Task.Delay(2000);
                                    // resetting variables
                                    accountNumberValidated = false;
                                    loginMenu = false;
                                    withdrawMenu = false;
                                    numbersEntered = "";
                                    accountNumber = "";
                                    pinNumber = "";
                                    txtDisplay.Text = "";
                                    pinCounter = 0;
                                    enterAccountNumber();
                                    return;
                                }
                                enterPinNumber();   // prompt for pin
                            }
                        }
                    }
                }
                else //User has logged in
                {
                    if (numbersEntered == null || numbersEntered.Length != 1) //Validate input
                    {
                        txtDisplay.Text = ("Invalid input, please try again!\r\n");
                        await Task.Delay(2000); //wait for 2 seconds
                        numbersEntered = null;
                        displayLoginMenu();
                    }
                    else //Valid choice
                    {
                        if (Convert.ToInt16(numbersEntered) == 1) //Withdraw money
                        {
                            withdrawMenu = true;
                            numbersEntered = null;
                            displayWithdrawMenu();

                        }
                        else if (Convert.ToInt16(numbersEntered) == 2) //Check balance
                        {
                            dispBalance();
                            numbersEntered = null;
                            await Task.Delay(2000); //wait for 2 seconds
                            if (!exit) //If cancel button has not been clicked
                            {
                                displayLoginMenu();
                            }
                        }
                        else if (Convert.ToInt16(numbersEntered) == 3) //Return card
                        {
                            txtDisplay.Text = ("Returning card, Goodbye!\r\n");
                            cardPic.Visible = true;
                            await Task.Delay(1500);
                            cardPic.Visible = false;
                            // resetting variables
                            accountNumberValidated = false;
                            loginMenu = false;
                            withdrawMenu = false;
                            numbersEntered = "";
                            accountNumber = "";
                            pinNumber = "";
                            txtDisplay.Text = "";
                            enterAccountNumber();
                        }
                        else //Invalid input
                        {
                            MessageBox.Show("Invalid entry", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            numbersEntered = null;
                            displayLoginMenu();
                        }
                    }
                }
            }
            else //Withdraw money
            {
                if (raceCondition == false) //Run version with semaphores
                {
                    semaphore.WaitOne();
                    bool successful = false;
                    txtDisplay.Text = "..........................................Loading..........................................";
                    await Task.Delay(10);
                    string toWrite;
                    string now = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");   // get time of transaction
                    writeLogInfo(now); //Write date to file
                    
                    if (Convert.ToInt32(numbersEntered) == 1) //Withdraw £10
                    {
                        successful = activeAccount.decrementBalance(10);
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Withdrew £10. \r\n");
                        writeLogInfo(toWrite); //Write to file
                    }
                    else if (Convert.ToInt32(numbersEntered) == 2) //Withdraw £20
                    {
                        successful = activeAccount.decrementBalance(20);
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Withdrew £20. \r\n");
                        writeLogInfo(toWrite);
                    }
                    else if (Convert.ToInt32(numbersEntered) == 3) //Withdraw £40
                    {
                        successful = activeAccount.decrementBalance(40);
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Withdrew £40. \r\n");
                        writeLogInfo(toWrite);
                    }
                    else if (Convert.ToInt32(numbersEntered) == 4) //Withdraw £100
                    {
                        successful = activeAccount.decrementBalance(100);
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Withdrew £100. \r\n");
                        writeLogInfo(toWrite);
                    }
                    else if (Convert.ToInt32(numbersEntered) == 5) //Withdraw £500
                    {
                        successful = activeAccount.decrementBalance(500);
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Withdrew £500. \r\n");
                        writeLogInfo(toWrite);
                    }
                    else //invalid entry
                    {
                        MessageBox.Show("Invalid entry", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        displayLoginMenu();
                        return;
                    }

                    if (successful == false) //Withdrawal was unsuccessful
                    {
                        MessageBox.Show("Insufficient Funds", "Withdrawal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Withdrawal error. Insufficient funds. \r\n");
                        writeLogInfo(toWrite);
                        numbersEntered = null;
                        withdrawMenu = false;
                        displayLoginMenu();
                    }
                    else if (successful == true) //Withdrew money
                    {
                        dispBalance();
                        moneyGif.Visible = true; //Show money gif
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Balance: £" + activeAccount.getBalance().ToString() + " \r\n \r\n");
                        writeLogInfo(toWrite); //Write to file
                        await Task.Delay(2500);
                        moneyGif.Visible = false;
                        displayLoginMenu();
                        numbersEntered = null;
                        withdrawMenu = false;
                    }
                    
                    semaphore.Release();
                }
                else //Run version without semaphores
                {
                    bool successful = false;
                    string toWrite;
                    string now = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");   // get time of transaction
                    writeLogInfo(now);
                    txtDisplay.Text = "..........................................Loading..........................................";

                    await Task.Delay(10);
                    if (Convert.ToInt32(numbersEntered) == 1) //Withdraw £10
                    {
                        successful = activeAccount.decrementBalance(10);
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Withdrew £10. \r\n");
                        writeLogInfo(toWrite);
                    }
                    else if (Convert.ToInt32(numbersEntered) == 2) //Withdraw £20
                    {
                        successful = activeAccount.decrementBalance(20);
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Withdrew £20. \r\n");
                        writeLogInfo(toWrite);
                    }
                    else if (Convert.ToInt32(numbersEntered) == 3) //Withdraw £40
                    {
                        successful = activeAccount.decrementBalance(40);
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Withdrew £40. \r\n");
                        writeLogInfo(toWrite);
                    }
                    else if (Convert.ToInt32(numbersEntered) == 4) //Withdraw £100
                    {
                        successful = activeAccount.decrementBalance(100);
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Withdrew £100. \r\n");
                        writeLogInfo(toWrite);
                    }
                    else if (Convert.ToInt32(numbersEntered) == 5) //Withdraw £500
                    {
                        successful = activeAccount.decrementBalance(500);
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Withdrew £500. \r\n");
                        writeLogInfo(toWrite);
                    }
                    else //invalid entry
                    {
                        MessageBox.Show("Invalid entry", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        displayLoginMenu();
                        return;
                    }

                    if (successful == false) //Withdrawal was unsuccessful
                    {
                        MessageBox.Show("Insufficient Funds", "Withdrawal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Withdrawal error. Insufficient funds. \r\n");
                        writeLogInfo(toWrite);
                        numbersEntered = null;
                        withdrawMenu = false;
                        displayLoginMenu();
                    }
                    else if (successful == true) //Withdrew money
                    {
                        dispBalance();
                        moneyGif.Visible = true; //Show money gif
                        toWrite = ("Account " + activeAccount.getAccountNum().ToString() + ": Balance: £" + activeAccount.getBalance().ToString() + " \r\n \r\n");
                        writeLogInfo(toWrite);
                        await Task.Delay(2000);
                        moneyGif.Visible = false;
                        displayLoginMenu();
                        numbersEntered = null;
                        withdrawMenu = false;
                    }
                }

            }
        }


        /*
         * Event handler for clear button - clears bu
         */
        private void BtnClearEvent_Click(object sender, EventArgs e)
        {
            numbersEntered = null;
            txtDisplay.Text = txtDisplay.Text.Remove(txtDisplay.Text.LastIndexOf(Environment.NewLine));
            txtDisplay.Text += "\r\n";
        }


        /*
         * Event handler for cancel button
         */
        private void BtnCancelEvent_Click(object sender, EventArgs e)
        {
            //Clear everything
            accountNumberValidated = false;
            loginMenu = false;
            withdrawMenu = false;
            numbersEntered = "";
            accountNumber = "";
            pinNumber = "";
            txtDisplay.Text = "";
            enterAccountNumber();
            exit = true; //cancel button has been clicked
        }


        /*
         * Method to prompt the user for their account number
         */
        private void enterAccountNumber()
        {
            txtDisplay.Text += ("Please enter your account number: " + "\r\n");
        }


        /*
         * Method to prompt the user for their pin number
         */
        private void enterPinNumber()
        {
            txtDisplay.Text += ("\r\nPlease enter your pin number: " + "\r\n");
        }


        /*
         * Method to display the menu to the user
         */
        private void displayLoginMenu()
        {
            exit = false; 
            txtDisplay.Text = ("");
            txtDisplay.Text = ("Welcome " + activeAccount.getAccountNum());
            txtDisplay.Text += ("\r\n\r\nPlease choose from the following menu items:\r\n>Press 1 to take money from your account\r\n>Press 2 to check your account balance\r\n>Press 3 to return card\r\n");
        }


        /*
         * Method to display the withdrawal menu to the user
         */
        private void displayWithdrawMenu()
        {
            txtDisplay.Text = ("");
            txtDisplay.Text = ("How much money would you like to withdraw?\r\n1) £10\r\n2) £20\r\n3) £40\r\n4) £100\r\n5) £500\r\n");

        }


        /*
         * Method to display the balance
         */
        private void dispBalance()
        {
            if (this.activeAccount != null)
            {
                txtDisplay.Text += ("\r\nYour current balance is : " + activeAccount.getBalance());
            }
        }


        /*
         * Account class
         */
        class Account
        {
            //the attributes for the account
            public int balance;
            private int pin;
            public int accountNum;

            // a constructor that takes initial values for each of the attributes (balance, pin, accountNumber)
            public Account(int balance, int pin, int accountNum)
            {
                this.balance = balance;
                this.pin = pin;
                this.accountNum = accountNum;
            }


            /*
             * Getter for balance
             */
            public int getBalance()
            {
                return this.balance;

            }


            /*
             * Setter for balance
             */
            public void setBalance(int newBalance)
            {
                this.balance = newBalance;
            }

            /*
             *   This funciton allows us to decrement the balance of an account
             *   it perfomes a simple check to ensure the balance is greater tha
             *   the amount being debeted
             *   
             *   returns:
             *   true if the transactions if possible
             *   false if there are insufficent funds in the account
             */
            public Boolean decrementBalance(int amount)
            {
                

                if (this.balance > amount)
                {
                    int tempBalance = balance;

                    Thread.Sleep(4000);
                    //await Task.Delay(2000);
                    tempBalance -= amount;
                    balance = tempBalance;
                    return true;
              
                }
                else
                {
                    return false;
                }
            }

            /*
             * This funciton check the account pin against the argument passed to it
             *
             * returns:
             * true if they match
             * false if they do not
             */
            public Boolean checkPin(int pinEntered)
            {
                if (pinEntered == pin)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }


            /*
            * Getter for accountNum
            */
            public int getAccountNum()
            {
                return accountNum;
            }
        }


        /*
         * Method to find and return an account
         */
        private Account findAccount()
        {
            for (int i = 0; i < ac.Length; i++)
            {
                if (ac[i].getAccountNum() == Convert.ToInt32(accountNumber))
                {
                    return ac[i];
                }
            }
            return null;
        }


        /*
         * Method to position and create a new form
         */
        private static void runThread()
        {
            ATM atm = new ATM();

            if (thrd == 1) //If it is the first form created
            {
                atm.StartPosition = FormStartPosition.Manual;
                atm.Location = new Point(100, 100); 
                thrd = 2;
            }
            else //position second form
            {
                atm.StartPosition = FormStartPosition.Manual;
                atm.Location = new Point(872, 100);
            }

            atm.Size = new Size(800, 900);
            atm.BackColor = Color.LightGray;
            Application.Run(atm);
        }


        /*
         * Method to initialise the account objects
         */
        private static void initialiseAccounts()
        {
            ac[0] = new Account(300, 1111, 111111);
            ac[1] = new Account(750, 2222, 222222);
            ac[2] = new Account(3000, 3333, 333333);
        }
        

        /*
         * Method to write log information (when money has been taken from accounts) to a text file 
         */
        private void writeLogInfo(string write)
        {
            fileSemaphore.WaitOne();
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(write);
                sw.Close();
            }
            fileSemaphore.Release();
        }

        
        /*
         * Main method
         */
        static void Main()
        {
            Application.Run(new CentralBank_Form());
            initialiseAccounts();

            File.WriteAllText(@"logInfo.txt", string.Empty); //Clear text file

            Thread[] atms = new Thread[numOfATMs];
            for (int i = 0; i < atms.Length; i++) //Create threads
            {
                atms[i] = new Thread(runThread);
                atms[i].Start();
            }
        }
    }
}
