using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Layouts;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using CommunityToolkit;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;

namespace SnowballCalculator;

public partial class MainPage : ContentPage
{
    //our list of data to update and change. You could also use an observable collection but in this case I opted to use a list and build the Grid in code
    private List<snowballData> data = new List<snowballData>();
    //variable to track contributions over time
    private double contribSt = 0;
    //variable to track interest over time
    private double interestSt = 0;
    //variable to check the time of the last text Entry input
    private TimeSpan lastInput;
    //variable to check if we are still waiting for input
    private bool isWaiting = false;
    //bool for milestone checking if Interest has surpassed our contributions, we track this in the data list but use this variable to mark when that the list data was set so we don't do it again
    private bool interestPassedContribution = false;
    //bool to check if we reached millionaire status, same as above we track this in the data list but use this variable to see if that was already set
    private bool millionaire = false;
    //Animation to set for UI
    private Animation anim;
    //Variable to set and check if already animating the pig during refresh 
    private bool isAnimating = false;
    //Items used in methods below to check for multiple inputs
    CancellationTokenSource tokenInput = new CancellationTokenSource();
    CancellationToken token = new CancellationToken();
    private List<MediaElement> mediaPl;
    private int currentMediaIndex = 0;
  
    public MainPage()
    {
        InitializeComponent();
        //add method for page load for initial setup
       this.Loaded += MainPage_Loaded;
        //set lastinput variable to now
        lastInput = DateTime.Now.TimeOfDay;
        
        
        
    }
    #region Bool Preferences for start
    //method to set preference for the includeStartContribution
    public bool getIncStartContribPref(bool? v)
    {
        //check to see if a value was passed to set or not
        if (v == null)
        {
            //if not value was passed just return value from pref if it exists
            var check = Preferences.Get("startContribPref", "false");
            return check == "true";
        }
        else
        {
            //if value was sent, set the value and run our calculations again
            Preferences.Set("startContribPref", v.ToString());
            updateData(null, null);
            return v.Value;
        }
    }
    //method to set preference for the startToday variable
    public bool getStartTodayPref(bool? v)
    {
        if (v == null)
        {
            //if not value was passed just return value from pref if it exists
            var check = Preferences.Get("startTodayPref", "false");
            return check == "true";
        }
        else
        {
            //if value was sent, set the value and run our calculations again
            Preferences.Set("startTodayPref", v.ToString());
            updateData(null, null);
            return v.Value;
        }
    }
    #endregion
    //method for animating our pig if we are updating data, just for fun
    public void refreshAnimation()
    {
        if (!isAnimating)
        {
            isAnimating = true;
            anim = new Animation(v => pigImage.Scale = v, 1, 1.25);
            anim.Commit(this, "PigAnimation", 60, 120, Easing.SpringIn, (v, c) => pigImage.Scale = 1, () => true);
        }
    }
    #region UI Animations
    public async void vertsAnimationSpecial(bool isStart)
    {
        //loop through our Border visual elements that hold our Text Entries and animate them.
        var verts = flexLayout.Children.OfType<Border>().ToList();
        if (isStart)
        {
            foreach (var v in verts)
            {
                await v.TranslateTo(0, 100, 0);
            }
        }

        //setup variable for animating the Border Element for our Text Entries
        uint tm = 1000;
        var startrot = -22.5;
        double rotVal = 0;
        int str = 0;
        double strr = -2;
        //loop through each element and animate it
        
        
        foreach (var v in verts)
        {
            //this is very hacked together to make elements look somewhat right. I should use math to do exact calculations based on width and position of element here but I was just having fun and not taking too serious.
            if (str == 0 || str == 3)
            {
                v.TranslateTo(0, -40, tm, Easing.SpringOut);
            }
            else
            {
                if (str == 2)
                {
                    v.TranslateTo(0, 6, tm, Easing.SpringOut);
                }
                else
                {
                    v.TranslateTo(0, 0, tm, Easing.SpringOut);
                }
            }
            str++;
            rotVal = startrot * strr;
            if (strr != 1)
            {
                if (strr == -2)
                {
                    v.RotateTo(50);
                }
                else
                {
                    v.RotateTo(rotVal);
                }

            }
            else
            {
                if (strr == 1)
                    v.RotateTo(-9);
            }

            strr++;
            if (strr == 0)
            {
                strr = 1;
            }
            tm = tm + 250;
        }
    }
    public async void vertAnimationDefault()
    {
        //loop through our Border visual elements that hold our Text Entries and animate them back to the non fancy view.
        var verts = flexLayout.Children.OfType<Border>().ToList();
        uint tm = 1000;
        var startrot = -22.5;
        double rotVal = 0;
        int str = 0;
        double strr = -2;
        //loop through each element and animate it
        foreach (var v in verts)
        {
            //this is very hacked together to make elements look somewhat right. I should use math to do exact calculations based on width and position of element here but I was just having fun and not taking too serious.
            if (str == 0 || str == 3)
            {
                if (str == 0)
                {
                    v.TranslateTo(-4, 8, tm, Easing.SpringOut);
                }
                else {
                    v.TranslateTo(0, 8, tm, Easing.SpringOut);
                }
            }
            else
            {
                if (str == 2)
                {
                    v.TranslateTo(0, 8, tm, Easing.SpringOut);
                }
                else
                {
                    v.TranslateTo(0, 8, tm, Easing.SpringOut);
                }
            }
            str++;
            rotVal = startrot * strr;
            if (strr != 1)
            {
                if (strr == -2)
                {
                    v.RotateTo(0);
                }
                else
                {
                    v.RotateTo(0);
                }

            }
            else
            {
                if (strr == 1)
                    v.RotateTo(0);
            }

            strr++;
            if (strr == 0)
            {
                strr = 1;
            }
            tm = tm + 250;
        }
    }
    #endregion
    private async void MainPage_Loaded(object sender, EventArgs e)
    {
        //setup our data on first run
        //set our checkbox values to the methods for preferences
        chkStartToday.IsChecked = getStartTodayPref(null);
        chkStartContrib.IsChecked = getIncStartContribPref(null);
        //setup variables for animating some things for fun
        borderView.Scale = 0;
        //set trademark value, feel free to change to your own
        txtTrademark.Text = DateTime.Now.Year.ToString() + " Pishah LLC.";
        //animate our company logo
        imgLogo.Scale = 0.5;
        //await animation of logo to finish
        await imgLogo.ScaleTo(0, 2000, Easing.SpringIn);
        //setup ui animation
        vertsAnimationSpecial(true);
        //animate compnay logon background opacity to fade out
        await splashGrid.FadeTo(0, 512, null);
        //remove our company element from app when done
        mainGrid.Remove(splashGrid);
        var st = 0;
        mediaPl = audioHolder.Children.OfType<MediaElement>().ToList();
        txtInterest.Focus();
    }
    

  
    private void playMedia() {
        mediaPl[currentMediaIndex].Stop();
        mediaPl[currentMediaIndex].Play();
        currentMediaIndex++;
        if (currentMediaIndex >= mediaPl.Count)
        {
            currentMediaIndex = 0;
        }
    }
    //method called when text Entry is changed
    private async void updateData(object sender, TextChangedEventArgs e)
    {
        playMedia();
        borderView.FadeTo(0.5, 250, Easing.SinIn);
        refreshAnimation();
        if (tokenInput != null) {
            if (tokenInput.Token.CanBeCanceled)
            {
                tokenInput.Cancel();
            }
        }
        //check for multiple quick text input entries
        var checkCanRun = await checkForMultipleInputs();
        if (checkCanRun)
        {
            //reset milestone variables to be checked again
            millionaire = false;
            interestPassedContribution = false;
            //set last time of entry to check later
            lastInput = DateTime.Now.TimeOfDay;
            if (!isWaiting)
            {
                //if we are doing calculations animate pig and set variable to know we are waiting for calculations to not loop over and over
                isWaiting = true;
                

            }
            //if the element holding our data has a scale of less than 1 animate it to be 1
            if (borderView.Scale < 1)
            {
                borderView.ScaleTo(1, 712, Easing.SpringOut);
            }
            await doCalculations();
        }
    }
    //method to check for multiple entries
    private async Task<bool> checkForMultipleInputs()
    {
        tokenInput = new CancellationTokenSource();
        //wait for 1024 milliseconds for another entry, if we cancel before this it will throw an error and return false.
        var toret = true;
        try
        {
            
            await Task.Delay(1024, tokenInput.Token);
        }
        catch (TaskCanceledException ex)
        {
            toret = false;
        }
        
        return toret;

    }
    public async Task doCalculations()
    {


        var dt = await setData();
        //lData.Clear();
        //foreach (var i in data) {
        //    lData.Add(i);
        //}
        //lData = data.ToObservableCollection();
        
        dataView.Content = dt;
        borderView.FadeTo(1, 250, Easing.SinIn);
        this.AbortAnimation("PigAnimation");
        isAnimating = false;
        isWaiting = false;


    }
    public async Task<Grid> setData()
    {
        //set the start value for contribution to add onto
        contribSt = 0;
        //set the start value for the interest to add onto
        interestSt = 0;
        //create the grid for our data, probabably be better to use a template to style easier etc but for my use here this was what I chose
        var dataGridContainer = new Grid
        {
            RowSpacing = 6,
            ColumnSpacing = 12,

            RowDefinitions =
            {
                new RowDefinition()
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition(),
                new ColumnDefinition()
            }
        };
        //set the first column width to be smaller to work with Mobile
        dataGridContainer.ColumnDefinitions[0].Width = 32;
        //go through the calculations async to not lock up the ui thread, although it still will
        await Task.Run(async () =>
        {
            //default start amount variable incase there is no input 
            double start = 0;
            //we use the TryParse from our Entry field to see if it is a number we can use
            double.TryParse(txtStart.Text, out start);
            if (chkStartContrib.IsChecked)
            {
                contribSt += start;
            }
            //default interest variable incase there is no input
            double interest = 0;
            double.TryParse(txtInterest.Text, out interest);
            if (interest != 0)
            {
                interest = interest * 0.01;
            }
            //default contribute variable incase there is no input
            double contribute = 0;
            double.TryParse(txtDaily.Text, out contribute);
            if (contribute != 0)
            {
                contribute = (21 * contribute) * 12;
            }
            //default age variable incase there is no input
            int age = 0;
            int.TryParse(txtAge.Text, out age);
            //clear our List of data out to start over building it
            data.Clear();

            //setup the loop
            var ind = 0;
            var end = 100;
            //setup a variable to build off of last looped item amount
            double prevEntry = (double)start;
            //check the age difference to standared retirement age, maybe best to allow user to change this
            if (age < 66)
            {
                end = 66 - age;
            }
            for (ind = 0; ind < end; ind++)
            {
                //Android was periodically having issues with the aync tasks when debugging, still not sure why, so a hack to catch errors. If you figure it out please let me know!
                try
                {
                    //call the method to calculate our values
                    var calculated = calculatedNumber(prevEntry, contribute, interest, ind, chkStartToday.IsChecked);
                    //set our variable to build off the amount off of
                    prevEntry = calculated.total;
                    //add the caclulated data row to our List of data
                    data.Add(new snowballData(age, calculated));
                    //update the age to +1
                    age++;
                }
                catch { }
            }
            //add the header for our Grid
            addGridHeader(dataGridContainer);
            //loop through our data and add each row based on the data, again probably better to use observable collection but I am new to this model and for me code is easier to understand
            var st = 1;
            for (int index = 0; index < data.Count(); index++)
            {
                try
                {
                    //check to see if data is valid, again issues with Android, not sure
                    if (index >= data.Count())
                    {

                    }
                    else
                    {
                        //add our grid row using a method
                        addGrid(dataGridContainer, data[index], st++);
                    }
                }
                catch { }
            }

        }).ConfigureAwait(false);
        //return our Grid
        return dataGridContainer;
    }
    public calculatedValue calculatedNumber(double amount, double contribution, double rate, int daysIndex, bool startToday)
    {
        //we are calculating our interest daily so we set the days to 365 here. Not sure this is the best way since we are technically doing 21 days per month but in my head easier to work with
        var days = 365;
        //if this is the first item in our data we check the variable to see if we should start based on today or beginning of the year
        if (daysIndex == 0 && startToday)
        {
            days = 365 - DateTime.Now.DayOfYear;
        }
        //set variables to check some milestones, millionaire and if interest has overtaken our contributions
        var isRich = false;
        var isInterestKing = false;
        //add our contribution amount, in this calculator it is always the same number so no need to add interest etc. If the variable to include the start amount was checked, the start value was already set to include that amount before we call the method.
        
        //set our variables for daily interest and contribution, these will ignore the start date always based on 365 days
        double interest = 0;
        double contrib = 0;
        var contD = contribution / 365;
        var rateD = rate / 365;
        //loop through the days to add each value
        var st = 0;
        var end = days;
        for (st = 0; st < end; st++)
        {
            contribSt += contD;
            var daily = (amount + contD) * rateD;
            interest += daily;
            interestSt += daily;
            contrib += contD;
            amount = (amount + contD) + daily;
        }
        //check our milestones and set the value if met and not already set
        if (!interestPassedContribution)
        {
            if (interestSt >= contribSt)
            {
                interestPassedContribution = true;
                isInterestKing = true;
            }
        }
        if (!millionaire)
        {
            if (amount >= 1000000)
            {
                millionaire = true;
                isRich = true;
            }
        }
        //return our calculated data
        return new calculatedValue { interest = interestSt, total = amount, contribution = contribSt, yearInterest = interest, rich = isRich, interestKing = isInterestKing };
    }
    public void addGridHeader(Grid dataGridContainer)
    {
        //create the Header row for our data
        dataGridContainer.RowDefinitions.Add(new RowDefinition());
        dataGridContainer.RowDefinitions[0].Height = 40;
        var lblAge = new Label();
        lblAge.Padding = new Thickness(0, 6);
        lblAge.Text = "Age:";
        var lblTotal = new Label();
        lblTotal.Padding = new Thickness(0, 6);
        lblTotal.Text = "Value:";
        var lblInterest = new Label();
        lblInterest.Padding = new Thickness(0, 6);
        lblInterest.Text = "Interest:";
        var lblContrib = new Label();
        var lblYearInterest = new Label();
        lblYearInterest.Text = "int/year:";
        lblYearInterest.Padding = new Thickness(0, 6);
        lblContrib.Text = "Contribution:";
        lblContrib.Padding = new Thickness(0, 6);
        dataGridContainer.Add(lblAge, 0, 0);
        dataGridContainer.Add(lblTotal, 1, 0);
        dataGridContainer.Add(lblInterest, 2, 0);
        dataGridContainer.Add(lblYearInterest, 3, 0);
        dataGridContainer.Add(lblContrib, 4, 0);
    }
    public void addGrid(Grid dataGridContainer, snowballData s, int index)
    {
        //the null check is again trying to figure out the Android issue, not sure can remove once we figure out the cause
        if (s != null)
        {
            dataGridContainer.RowDefinitions.Add(new RowDefinition());
            var lblAge = new Label();
            lblAge.Text = s.age.ToString();
            var lblTotal = new Label();
            //check millionaire milestone and make bold if true
            if (s.rich)
            {
                lblTotal.FontFamily = "InterBold";
            }
            lblTotal.Text = s.total.ToString("C", CultureInfo.CurrentCulture);
            var lblInterest = new Label();
            lblInterest.Text = s.interest.ToString("C", CultureInfo.CurrentCulture);
            var lblYearInterest = new Label();
            lblYearInterest.Text = s.yearInterest.ToString("C", CultureInfo.CurrentCulture);
            var lblContrib = new Label();
            //check variable to see if our interest sum has passed our contribution sum and set fonts to bold to show
            if (s.interestKing)
            {
                lblInterest.FontFamily = "InterExtraBold";
                lblContrib.FontFamily = "InterBold";
            }
            lblContrib.Text = s.contribution.ToString("C", CultureInfo.CurrentCulture);
            dataGridContainer.Add(lblAge, 0, index);
            dataGridContainer.Add(lblTotal, 1, index);
            dataGridContainer.Add(lblInterest, 2, index);
            dataGridContainer.Add(lblYearInterest, 3, index);
            dataGridContainer.Add(lblContrib, 4, index);

        }
    }



    private void chkStartToday_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        //set preference value for whether to build off of today or beginning or year
        getStartTodayPref(e.Value);
    }

    private void chkStartContrib_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        //set prefererence value for if the starting amount should be included in our contributions
        getIncStartContribPref(e.Value);
    }

    private void toggleView_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            vertAnimationDefault();
        }
        else
        {
            {
                vertsAnimationSpecial(false);
            }
        }
    }

    private async void ClickGestureRecognizer_Clicked(object sender, EventArgs e)
    {
        Uri uri = new Uri("https://www.microsoft.com");
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }

    private void PointerGestureRecognizer_PointerEntered(object sender, PointerEventArgs e)
    {
        txtTrademark.TextColor = Colors.Black;
        txtTrademark.FontAttributes = FontAttributes.Italic;
    }

    private void PointerGestureRecognizer_PointerExited(object sender, PointerEventArgs e)
    {
        
        txtTrademark.TextColor = Colors.White;
        txtTrademark.FontAttributes = FontAttributes.None;
    }
}
public class snowballData
{

    public int age { get; set; }
    public double total { get; set; }
    public double interest { get; set; }
    public double yearInterest { get; set; }
    public double contribution { get; set; }
    public bool rich { get; set; }
    public bool interestKing { get; set; }
    public string totalBold { get; set; }
    public string interestBold { get; set; }
    public snowballData(int age, calculatedValue val)
    {

        this.total = val.total;
        this.interest = val.interest;
        this.contribution = val.contribution;
        this.yearInterest = val.yearInterest;
        this.rich = val.rich;
        this.interestKing = val.interestKing;
        if (val.rich)
        {
            totalBold = "Bold";
        }
        else {
            totalBold = "None";
        }
        if (val.interestKing)
        {
            interestBold = "Bold";
        }
        else { 
            interestBold= "None";
        }
        this.age = (int)age++;
    }

    public snowballData() { }

}
public class calculatedValue
{
    public double total { get; set; }
    public double interest { get; set; }
    public double yearInterest { get; set; }
    public double contribution { get; set; }
    public bool rich { get; set; }
    public bool interestKing { get; set; }
}

