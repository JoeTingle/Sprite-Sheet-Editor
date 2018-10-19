using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using Tutorial;
//Library For Colour Picker
using Xceed;
//Library For JSON
using Newtonsoft;
using SaveLibrary;

namespace WpfApp1
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Main bitmap source that everything will be drawn to
        WriteableBitmap backBuffer;
        //Contains the list of all of the layers/sprites
        private ViewModel viewModel = new ViewModel();
        //Keeps track of the currently selected sprite in the viewmodel
        private Sprite currentSprite;
        //The current selected colour through the colour picker on XAML
        private Color color;
        //Keeps track of which mode is selected, none are by default
        bool DrawSelected = false;
        bool MoveSelected = false;
        //Keeps track on when the user has pressed the mouse button for drawing/moving
        private bool mouseDown = false;
        
        public MainWindow()
        {
            //Initialise the form
            InitializeComponent();

            this.DataContext = viewModel;

            Loaded += delegate {

                //Creating the first bitmap layer and setting the default draw colour to black, incase the user doesn't select another.
                backBuffer = BitmapFactory.New((int)imgBorder.ActualWidth, (int)imgBorder.ActualHeight);
                color = Colors.Black;
                ClrPcker_Background.SelectedColor = color;
                UpdateBitmap();

            };
        }

        private void UpdateBitmap()
        {
            backBuffer.Clear(Colors.White);

            foreach (Sprite sprite in viewModel.SpriteList)
            {
                backBuffer.Blit(
                    new Rect(sprite.X, sprite.Y, sprite.Image.Width, sprite.Image.Height),
                    sprite.Image,
                    new Rect(0, 0, sprite.Image.Width, sprite.Image.Height)
                );
            }

            MainImage.Source = backBuffer;
        }

        private void ImgMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = true;
            //Added these from the mouse move function as it allows the user to click once and it will still add a pixel without having to move the mouse
            Point mousePosition = e.GetPosition(MainImage);
            if (DrawSelected)
            {
                backBuffer.SetPixel((int)mousePosition.X, (int)mousePosition.Y, color);
            }
            if (MoveSelected)
            {
            }
            else if (!DrawSelected && !MoveSelected)
            {
                MessageBox.Show("Please select a layer to draw on and either draw or move !");
            }
        }

        private void ImgMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
        }

        private void ImgMain_MouseMove(object sender, MouseEventArgs e)
        {
            //Creating a mouse position to get accurate drawing and movement
            Point mousePosition = e.GetPosition(MainImage);
            int posX = Convert.ToInt32(mousePosition.X);
            //Setting UV coord text in XAML
            XPos.Text = Convert.ToString(posX);
            int posY = Convert.ToInt32(mousePosition.Y);
            YPos.Text = Convert.ToString(posY);
            if (!mouseDown)
            {
                return;
            }
            if (DrawSelected)
            {
                if (mouseDown)
                {

                    backBuffer.SetPixel((int)mousePosition.X, (int)mousePosition.Y, color);
                }
            }
            if (MoveSelected)
            {
                //null check on current sprite, so you can't move the base canvas
                if (currentSprite != null)
                {
                    currentSprite.X = mousePosition.X;
                    currentSprite.Y = mousePosition.Y;
                    UpdateBitmap();
                }
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            //Displaying dialog box to choose a file to laod
            OpenFileDialog op = new OpenFileDialog();
            Sprite sprite = new Sprite();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*.bmp|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png|" +
              "Bitmap file format (*.bmp)|*.bmp";
            if (op.ShowDialog() == true)
            {
                //Creating the new bitmap with the selected image, by converting it
                sprite.Image = BitmapFactory.ConvertToPbgra32Format(new BitmapImage(new Uri(op.FileName)));
            }

            sprite.Path = op.FileName;
            //This sets bitmapname to the path, without the directories, and without the extension
            sprite.Name = op.FileName.Split(System.IO.Path.DirectorySeparatorChar).Last().Split('.').First();

            //backBuffer = backBuffer.Resize((int) imgBorder.ActualWidth, (int) imgBorder.ActualHeight, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
            viewModel.SpriteList.Add(sprite);

            UpdateBitmap();

        }

        private void BtnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            //Brings up another dialog box for the user to choose a path to save the file
            //Same formats as loading dialog
            SaveFileDialog save = new SaveFileDialog
            {
                Title = "Save your picture",
                Filter = "All supported graphics | *.jpg; *.jpeg; *.png; *.bmp | " +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png|" +
              "Bitmap file format (*.bmp)|*.bmp"
            };
            if (save.ShowDialog() == true)
            {
                SaveBitmap(save.FileName, backBuffer.Clone());
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            //Immediatly saves it to the desktop as a bmp file format
            SaveBitmap("C:\\Users\\Public\\Desktop\\Image.bmp", backBuffer.Clone());
        }


        void SaveBitmap(string filename, BitmapSource image)
        {
            //Using filestream it creates a new png using the bitmap and saves it do the desired location
            if (filename != string.Empty)
            {
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    PngBitmapEncoder bmp = new PngBitmapEncoder();
                    bmp.Frames.Add(BitmapFrame.Create(image));
                    bmp.Save(stream);
                }
            }
        }

        void MnuFileExit_Click(object sender, RoutedEventArgs e)
        {
            //Immediatly closes the application
            System.Windows.Application.Current.Shutdown();
        }

        private void BtnFileNew_Click(object sender, RoutedEventArgs e)
        {
            //Clears all sprites/layers from the viewmodel and resets the canvas colour
            //This creates the new canvas
            viewModel.SpriteList.Clear();
            backBuffer.Clear(Colors.White);
            backBuffer = backBuffer.Resize((int)MainImage.ActualWidth, (int)MainImage.ActualHeight, WriteableBitmapExtensions.Interpolation.NearestNeighbor);


            UpdateBitmap();
        }

        private void AddNewLayer(object sender, RoutedEventArgs e)
        {
            //Enables the popup for the user to enter a custom name for the sprite/layers
            PopupWindow popup = new PopupWindow();
            popup.ShowDialog();
            string Name = popup.Name;

            //Then creates a new sprite with that name passed in and adds it to the view model
            Sprite sprite = new Sprite
            {
                Image = BitmapFactory.New((int)backBuffer.Width, (int)backBuffer.Height),
                Name = Name
            };
            viewModel.SpriteList.Add(sprite);

            UpdateBitmap();
        }

        private void RemoveNewLayer(object sender, RoutedEventArgs e)
        {
            //Removes the currently selected sprite from the viewmodel
            viewModel.SpriteList.Remove(viewModel.SpriteList[ListView.SelectedIndex]);
        }

        private void BtnEditCut_Click(object sender, RoutedEventArgs e)
        {
            //Copies the canvas(backbuffer) and it's contents to the clipboard, to be pasted elsewhere
            Clipboard.SetImage(backBuffer);
            //Then clears the canvas
            backBuffer.Clear(Colors.White);
        }

        private void BtnEditPaste_Click(object sender, RoutedEventArgs e)
        {
            //Currently Not Working
            //Convert the cipboard image back to a readable format
            backBuffer = BitmapFactory.ConvertToPbgra32Format(Clipboard.GetImage());  
        }

        private void BtnEditCopy_Click(object sender, RoutedEventArgs e)
        {
            //Copies the canvas(backbuffer) and it's contents to the clipboard, to be pasted elsewhere
            Clipboard.SetImage(backBuffer);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //This function currently isn't being called

            //On window resize i want to re calibrate the mouse so that it is in the correct position.
            backBuffer = BitmapFactory.New((int)imgBorder.ActualWidth, (int)imgBorder.ActualHeight);
        }

        private void MnuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            //Could display some text here, about the program
            MessageBox.Show("Sprite Sheet Editor \n\n ~Joe", "About The Program");
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //checks if the current index is out of range
            //if so it resets it back to 0, the first element
            if (ListView.SelectedIndex < 0)
            {
                ListView.SelectedIndex = 0;
            }
            else
            {
                //setting the current sprite to draw on as the selected index in the viewmodel
                currentSprite = viewModel.SpriteList[ListView.SelectedIndex];
            }
        }

        public void ClrPcker_Background_SelectedColorChanged(object sender, RoutedEventArgs e)
        {
            //Using the colour picker to select any colour to draw with
            color = (Color)ClrPcker_Background.SelectedColor;
        }

        private void Drawbtn_Click(object sender, RoutedEventArgs e)
        {
            MoveSelected = false;
            DrawSelected = true;
            

        }

        private void Movebtn_Click(object sender, RoutedEventArgs e)
        {
            DrawSelected = false;
            MoveSelected = true;
            

        }

        private void BtnCanvasFill_Click(object sender, RoutedEventArgs e)
        {
            //Setting the background colour of the canvas to the currently selected colour on the colour picker
            backBuffer.Clear(color);
        }
    }
}
