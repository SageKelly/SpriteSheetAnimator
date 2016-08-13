using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace SpriteSheetAnimator
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        /*PLEASE OPEN EVERY REGION!!!!! THERE ARE COMMENTS, EXPLAINING
         * HOW EVERYTHING WORKS, ALL THROUGHOUT THESE REGIONS. IF 
         * YOU DON'T, YOU'LL BE SORRY!!!
         */
        #region Declared Variables
        #region Enums
        //These are the different interfaces for the GraphicsSelection state.
        enum Focuses
        {
            FPS,
            Frames,
            Offset,
            ViewSize,
            Scale
        }

        //These are the different states in the program.
        enum AnimStates
        {
            TitleScreen,
            FolderSelection,
            GraphicsSelection,
            LoadingElems,
            AnimationModding,
        }

        Focuses Focus;
        AnimStates AS;
        /*This is used for the program to distinguish state change from FolderSelection 
         * and GraphicsSelection states to the LoadingPic state. If coming from the 
         * FolderSelection different actions will be taken, as opposed to GraphicsSelection 
         * access.
         */
        AnimStates PrevAS;
        #endregion

        #region Objects
        public delegate void ButtonEventHandler(int Offset, bool Dir);
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        //This will create a random color value for each element of the BG_Color (see below).
        Random randColor = new Random();

        //This will save your default directory, which you can choose, to a file.
        StreamWriter DefaultDirectoryWriter;

        //Once running, the program will allow you to use the saved default directory.
        StreamReader DirectoryReader;

        /*This will write the history of your directory traversal to a file once your 
         * default folder has been selected. This is done so that once you return 
         * to the program you are not "stuck" on one folder, and can now 
         * return to the system's default folder.
         */
        StreamWriter StackWriter;

        //This fetches the saved directory traversal. It will later be saved into a list during runtime.
        StreamReader StackReader;
        InputHandler input;
        IInputHandler Iinput;

        /*This is used to both grab the sprites need for the GUI, and the 
         * picture selected from the GraphicsSelection state.
         */
        FileStream PicGetter;
        Color BG_Color;
        Color TextColor = Color.White;

        /*This will hold the full name of the directories, which will be later 
         * referenced for both folder traversal, as well as graphic selection. 
         * It is repurposed between the FolderSelection and GraphicsSelection states.
         */
        List<string> DirList = new List<string> { };

        /*This holds the history of your folder explorations. You can use this to move back 
         * through your directories to another folder.
         */
        List<string> DirStack = new List<string> { };

        /*Used specifically for graphical purposes, this holds only the filenames, 
         * instead of the full directory name. You will be printing these to the screen.
         * It is repurposed between the FolderSelection and GraphicsSelection states.
         */
        List<FilenameText> FilenameList = new List<FilenameText> { };

        //Later used during the graphic selection and animation modulation sections.
        Texture2D SelectedPic;

        //GUI sprites---------------------
        Texture2D SelectionBar;
        Texture2D SelectionBorder;
        Texture2D WhiteBG;
        Texture2D FolderBar;
        Texture2D UpArrow;
        Texture2D DownArrow;
        Texture2D RightArrow;
        Texture2D LeftArrow;
        //----------------------------------------------------

        /*
         * Used for altering the appearance of the selected sprite. Vector2 objects 
         * are used for easier memory storage and variable naming.
         */
        Vector2 Offset = new Vector2();
        Vector2 ViewSize = new Vector2(50, 50);

        //The viewing bounds for the selected sprite
        Rectangle AnimRect;

        //The viewing bounds that shows a large view of the sprite sheet
        Rectangle GlobalViewRect;

        /*Used for drawing the text to the screen. If it's within the 
         * bounds of this viewport, then draw it.
         */
        Rectangle ViewportRect;



        //Used to cycle colors behind the spritesheet's viewport.
        Rectangle ColorRect;

        //Timer used for animation. Controlled by the FPS setting.
        TimeSpan AnimTime = new TimeSpan(0);

        /*Used for dynamic list-scrolling. The longer you hold down button, 
         * the faster the list scrolls, up to three times the normal speed.
         */
        TimeSpan LeftButtonDownTime = new TimeSpan(0);
        TimeSpan RightButtonDownTime = new TimeSpan(0);
        TimeSpan DownButtonDownTime = new TimeSpan(0);
        TimeSpan UpButtonDownTime = new TimeSpan(0);

        /*Used for determining the speed of the list's scrolling. 
         * As it passes certain time checkpoints, the scrolling speed will augment itself.
         */
        TimeSpan ButtonIncTime = new TimeSpan(0);

        //Used to add the technical, or singly-incremental, values to the offset.
        Vector2 iTechnical_offset = Vector2.Zero;


        #endregion
        #region Primitives
        /*Colors for the Background. The background will slowly 
        * change colors in an effort to reveal dangling pixels.
         */
        byte red, green, blue;

        /*These are used to check if each of the color components
         * are moving to zero. More explanation in
         * AnimationModding Handling State
         */
        bool r_to_0, g_to_0, b_to_0;

        /// <summary>
        ///This is the default directory written to file.
        /// </summary>
        string sDefaultDir = @"C:\";

        /// <summary>
        /// Represents the default file directory.
        /// </summary>
        static string sFileDir = @"Text Files";

        /// <summary>
        ///This is the location of the directory save file.
        /// </summary>
        string sDefaultDirFile = sFileDir + @"\DefaultDir.txt";

        /// <summary>
        /// This is the location of the directory stack file.
        /// </summary>
        string sStackFile = sFileDir + @"\FileStack.txt";

        //This is used to control which is loaded from the directory list.
        //This one controls the FolderSelection's selected directory.
        string sSelectedDir;
        //This one controls the GraphicsSelection's selected directory.
        string sSelectedPic;
        /*THIS MUST STAY AT 1000f, IN ORDER TO KEEP IT FROM 
         * ANIMATING TOO FAST WHEN IT STARTS OFF
         */
        float fFPS = 1000f;
        int iSelection;
        /*This is used to decide between if the user wants to 
         * step into the folder, or choose it as his default.
         */
        int iFolderDecision;
        int iFPS = 1;
        int iFrameInc;
        int iFrames = 1;
        int iTextOffset;
        int iScrollbarOffset;
        private int iStateInc = 1;
        private int iFocusInc = 0;
        float fPicScale = 1;
        const float fMAX_SCALE = 10.0f;

        /*I doubt anyone would need a sensibly need a framrate faster than this. 
         * The most common framerate in TV productions is 24fps, 
         * with 25fps being used in Canada.
         */
        const int iMAX_FPS = 50;

        //This is the spacing for the title of the screen.
        const int iHEADER_SPACING = 40;

        //This is the spacing used for the list's content.
        const int iTEXT_SPACING = iHEADER_SPACING / 2;

        /*This is the initial scrolling delay: you have to be holding the 
         * particular button at least this long before auto-scroll commences.
         */
        const int iDELAY = 750;

        /*These are the delay times for the different scrolling speeds. 
         * Each one is 5pps (pixels per second) faster than the last.
         */
        const int iSLOW_INC = 100; //1000 / 10
        const int iMED_INC = 66; //10000 / 15
        const int iFAST_INC = 50; //1000 / 20

        /*
         * Used with the above variables (SLOW_INC, MED_INC, etc.) to
         * change between the times for extra abstraction.
         */
        int iTimeSwitch = iDELAY;

        //This is used so LoadContent() is called at the appropriate times in each state.
        bool bHasLoaded;

        //Used to allow for techinical offset that is not incremented by the size of the AnimRect;
        bool bTech_offset_on;


        //Screen Dimension constants: DO NOT CHANGE!!!
        const int SCRN_HEIGHT = 600, SCRN_WIDTH = 800;

        #endregion
        #endregion

        #region Game1()
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            input = new InputHandler(this);
            Components.Add(input);
            Iinput = (IInputHandler)this.Services.GetService(typeof(IInputHandler));

            graphics.PreferredBackBufferHeight = SCRN_HEIGHT;
            graphics.PreferredBackBufferWidth = SCRN_WIDTH;


            ViewportRect = new Rectangle(0, iHEADER_SPACING, SCRN_WIDTH,
                SCRN_HEIGHT - (iHEADER_SPACING * 2));
            #region Create/Read Default File
            if (!File.Exists(sDefaultDirFile))
            {
                //Check to see if the directory does
                if (!Directory.Exists(sFileDir))
                {
                    Directory.CreateDirectory(sFileDir);
                }
                //Create the file. 
                DefaultDirectoryWriter = new StreamWriter(sDefaultDirFile, false);
                //Write the system's default directory to the file.
                DefaultDirectoryWriter.Write(sDefaultDir);
                DefaultDirectoryWriter.Close();
            }
            else
            {
                //If it does exist, read the default from the file
                DirectoryReader = new StreamReader(sDefaultDirFile);
                //Save it to a local variable.
                sDefaultDir = DirectoryReader.ReadLine();
                DirectoryReader.Close();
                /*This, along with the previous if statement, is done to prevent 
                 * the program from writing over the saved default directory.
                 */
            }
            #endregion
            #region Create Stack File
            if (!File.Exists(sStackFile))
            {
                /*
                 * There comes a time when the directory chosen will show no files in
                 * the folder selection state. Also, it does not allow for any further traversal
                 * through your directories. By saving the directory stack, it allows the
                 * user to move back through their directories to a different folder.
                 */
                StackWriter = new StreamWriter(sStackFile, false);
                StackWriter.Close();
                /*We also use this if statement for the creation of a stack file.*/
            }
            #endregion
            /*
             * This will move the saved directory traversal to a list for use during the
             * FolderSelection state.
             */
            StackReader = new StreamReader(sStackFile);
            while (!StackReader.EndOfStream)
            {
                DirStack.Add(StackReader.ReadLine());
            }
            StackReader.Close();
            /*
            if (sDefaultDir == null)
                sDefaultDir = @"C:\";
             * Debug Code to be removed later.
             * Activate this should you decide to
             * mess with the SaveDefaultDir() method.
             */
            sSelectedDir = sDefaultDir;
        }
        #endregion

        #region Game Methods
        #region Initialize()
        protected override void Initialize()
        {
            Iinput.PrevKBState = Keyboard.GetState();

            red = (byte)randColor.Next(0, 256);
            green = (byte)randColor.Next(0, 256);
            blue = (byte)randColor.Next(0, 256);

            if (red > 0)
                r_to_0 = false;
            else
                r_to_0 = true;
            if (green > 0)
                g_to_0 = false;
            else
                g_to_0 = true;
            if (blue > 0)
                b_to_0 = false;
            else
                b_to_0 = true;


            bHasLoaded = false;

            base.Initialize();
        }
        #endregion
        #region LoadContent()
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            #region Folder Selection
            if (AS == AnimStates.FolderSelection)
            {
                iSelection = 0;
                iTextOffset = 0;
                try
                {
                    DirList.AddRange(Directory.EnumerateDirectories(sSelectedDir));
                }
                catch (UnauthorizedAccessException)
                {
                    /*This is mainly for if you try to access a folder you're not supposed to 
                     * be in. This will prevent you from accessing it by removing your current 
                     * directory, referring to inside the blocked folder, from the directory 
                     * traversal list, and returning you to the previous directory, which will 
                     * just make the screen blink and return you to the top of the current list.
                     */
                    if (DirStack.Count != 1)
                    {
                        DirStack.RemoveAt(DirStack.Count - 1);
                        sSelectedDir = DirStack[DirStack.Count - 1];
                    }
                    DirList.AddRange(Directory.EnumerateDirectories(sSelectedDir));
                }
                catch (IOException)
                {
                    /*When you return to the FolderSelection state via the AnimationModding
                     * state, you run into an issue where the program will attempt to treat a 
                     * file like a directory. When that happens, this is the error thrown. This 
                     * will simply adjust your selected directory so that it's suitable for 
                     * directory traversal in the FolderSelection, as opposed to the
                     * GraphicsSelection state, which is file traversal.
                     */
                    sSelectedDir = DirStack[DirStack.Count - 1];
                    try
                    {
                        DirList.AddRange(Directory.EnumerateDirectories(sSelectedDir));
                    }
                    catch (DirectoryNotFoundException)
                    {
                        /*
                         * When changing directory names, this error can occur. At this point,
                         * we have to adjust the directory to a more accurate one.
                         */
                        string[] temp = sSelectedDir.Split('\\');
                        //TODO: Flesh out this code.
                    }
                }

                foreach (string s in DirList)
                {
                    /*Let only the filenames be shown, instead of the directory. Thes are printed to 
                     * the screen, while the contents of the DirList are used for directory traversal.
                     */
                    FilenameList.Add(new FilenameText(Path.GetFileName(s), new Vector2(
                        0, iHEADER_SPACING + (FilenameList.Count * iTEXT_SPACING)), FilenameList.Count));
                }
            }
            #endregion
            #region Graphics Selection
            else if (AS == AnimStates.GraphicsSelection)
            {
                iSelection = 0;
                iTextOffset = 0;

                //Only load these three file types
                DirList.AddRange(Directory.EnumerateFileSystemEntries(sSelectedDir, "*.bmp").ToList<string>());
                DirList.AddRange(Directory.EnumerateFileSystemEntries(sSelectedDir, "*.gif").ToList<string>());
                DirList.AddRange(Directory.EnumerateFileSystemEntries(sSelectedDir, "*.png").ToList<string>());

                foreach (string s in DirList)
                {
                    //Let only the filenames be shown, instead of the directory.
                    FilenameList.Add(new FilenameText(Path.GetFileName(s), new Vector2(
                        0, iHEADER_SPACING + (FilenameList.Count * iTEXT_SPACING)), FilenameList.Count));
                }
            }
            #endregion
            #region Loading Pictures
            else if (AS == AnimStates.LoadingElems)
            {
                //Fetches the picture selected from the GraphicsSelection state
                PicGetter = new FileStream(sSelectedPic, FileMode.Open);
                SelectedPic = Texture2D.FromStream(this.GraphicsDevice, PicGetter);
                if (SelectedPic.Bounds.Width > SCRN_WIDTH)
                    GlobalViewRect = new Rectangle(0, (SCRN_HEIGHT / 2) +
                        (SCRN_HEIGHT / 4),
                        SCRN_WIDTH, SelectedPic.Height);
                else
                    GlobalViewRect = new Rectangle(0, (SCRN_HEIGHT / 2) +
                        (SCRN_HEIGHT / 4),
                        SelectedPic.Width, SelectedPic.Height);
            }
            #endregion
            #region Default
            else
            {
                //This loads all of the GUI sprites.
                PicGetter = new FileStream(@"Content\Assets\FolderBar.png", FileMode.Open);
                FolderBar = Texture2D.FromStream(this.GraphicsDevice, PicGetter);

                PicGetter = new FileStream(@"Content\Assets\HighlightBar.png", FileMode.Open);
                SelectionBar = Texture2D.FromStream(this.GraphicsDevice, PicGetter);


                PicGetter = new FileStream(@"Content\Assets\Up Arrow.png", FileMode.Open);
                UpArrow = Texture2D.FromStream(this.GraphicsDevice, PicGetter);


                PicGetter = new FileStream(@"Content\Assets\Down Arrow.png", FileMode.Open);
                DownArrow = Texture2D.FromStream(this.GraphicsDevice, PicGetter);


                PicGetter = new FileStream(@"Content\Assets\Right Arrow.png", FileMode.Open);
                RightArrow = Texture2D.FromStream(this.GraphicsDevice, PicGetter);


                PicGetter = new FileStream(@"Content\Assets\Left Arrow.png", FileMode.Open);
                LeftArrow = Texture2D.FromStream(this.GraphicsDevice, PicGetter);

                PicGetter = new FileStream(@"Content\Assets\Selection Border.png", FileMode.Open);
                SelectionBorder = Texture2D.FromStream(this.GraphicsDevice, PicGetter);

                PicGetter = new FileStream(@"Content\Assets\White BG.png", FileMode.Open);
                WhiteBG = Texture2D.FromStream(this.GraphicsDevice, PicGetter);

                font = Content.Load<SpriteFont>(@"Assets\Font");

                AnimRect = new Rectangle((int)ViewSize.X * iFrameInc + (int)Offset.X, (int)Offset.Y, (int)ViewSize.X, (int)ViewSize.Y);
                ColorRect = AnimRect;
                //This is to declare what state on which the program will start.
                AS = AnimStates.FolderSelection;
            }
            #endregion
            PicGetter.Close();
        }
        #endregion
        #region UnloadContent()
        protected override void UnloadContent()
        {
            /*This is used between selections in the FolderSelection state. This clears
             * the lists so that they can be repopulated once the LoadContent() 
             * method is called.
             */
            DirList.Clear();
            FilenameList.Clear();
        }
        #endregion
        #region Update()
        protected override void Update(GameTime gameTime)
        {
            HandleStates(gameTime);
            base.Update(gameTime);
        }
        #endregion
        #region Draw()
        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
        #endregion
        #endregion

        #region My Methods
        #region HandleStates()
        public void HandleStates(GameTime gameTime)
        {
            Iinput.KBState = Keyboard.GetState();
            switch (AS)
            {
                #region Folder Selection
                case AnimStates.FolderSelection:
                    while (!bHasLoaded)
                    {
                        LoadContent();
                        bHasLoaded = true;
                    }

                    spriteBatch.Begin();
                    GraphicsDevice.Clear(Color.Blue);
                    /*Draws the general text to the screen.*/
                    spriteBatch.DrawString(font, "Folder Selection: Choose your default folder.", Vector2.Zero, TextColor);

                    spriteBatch.DrawString(font, "Open Folder", new Vector2(
                        0, SCRN_HEIGHT - iTEXT_SPACING - (iTEXT_SPACING / 2)), TextColor);

                    spriteBatch.DrawString(font, "Select", new Vector2(
                        SCRN_WIDTH / 2,
                        SCRN_HEIGHT - iTEXT_SPACING - (iTEXT_SPACING / 2)), TextColor);

                    #region Drawing the file names
                    /*Draw the each of the filenames to their appropriate places. 
                     * Each of the object's positions were created when they were added to the list. 
                     * The TEXT_SPACING is used for standard spacing between each instance. 
                     * TextOffset if used for scrolling the filenames so that they move at the appropriate times. 
                     * HEADER_SPACING is the gap for the general text at the top of the screen.
                     */
                    foreach (FilenameText FT in FilenameList)
                    {
                        FT.PosY = ((iTEXT_SPACING * FT.ListPosition) + iTextOffset + iHEADER_SPACING);
                    }
                    /* This is used so that the filenames print in the correct spot on the screen,
                     * else, once the filenames scroll, they will print over the general text.
                     */
                    foreach (FilenameText FT in FilenameList)
                    {
                        if (ViewportRect.Contains(new Point((int)FT.PosX, (int)FT.PosY)))
                            spriteBatch.DrawString(font, FT.Text, new Vector2(FT.PosX, FT.PosY), TextColor);
                    }

                    //-----------------------------------------------selection bars---------------------------------------------------
                    /*Not much to address here, except for the ScrollbarOffset variable, which is used so the 
                     * position of the scrollbar will be in the same position as the currently selected folder. The
                     * 4 just makes the selection bar look better on the screen.
                     */
                    if (FilenameList.Count != 0)
                        spriteBatch.Draw(SelectionBar, new Vector2(0, FilenameList[iScrollbarOffset].PosY + 4), Color.White);

                    /*This is for the two decisions at the bottom of the screen: Open Folder and Select Folder.
                     * One is positioned at 0 on the X axis, and the other is in the middle of the screen on the X axis.
                     */
                    spriteBatch.Draw(FolderBar, new Vector2(iFolderDecision * (SCRN_WIDTH / 2),
                        SCRN_HEIGHT - iTEXT_SPACING - (iTEXT_SPACING / 2) + 4), Color.White);
                    //-------------------------------------------------------------------------------------------------------------------
                    #endregion

                    spriteBatch.End();
                    #region Pressing Enter
                    if (Iinput.KBState.IsKeyDown(Keys.Enter) && Iinput.PrevKBState.IsKeyUp(Keys.Enter))
                    {
                        /*The current folder decision will decide what will happen this current
                         * input. If you decide to just open the folder (0), then the program will
                         * just step into the folder. However, should you decide to select the folder
                         * (1), the program will set it as your default and move to the next state. 
                         * More versatility will be added later that will allow for the user to select the
                         * folder, but not necessarily make it the default.
                         */
                        if (iFolderDecision == 0)
                        {
                            if (FilenameList.Count != 0)
                            {
                                iTextOffset = 0;
                                iScrollbarOffset = 0;
                                PrevAS = AS;
                                AS = AnimStates.LoadingElems;
                                /*This is used to allow the LoadContent() method to be called once during
                                 * this state, instead of each frame.
                                 */
                                bHasLoaded = false;
                            }
                        }
                        else
                        {
                            if (FilenameList.Count != 0)
                            {
                                //Reset the FolderDecision sprite to the left side of the screen for the FolderSelection state.
                                iFolderDecision = 0;
                                //Reset scrolling values for the next menu.
                                iTextOffset = 0;
                                iScrollbarOffset = 0;

                                PrevAS = AS;
                                CycleStatesRight();
                                SaveDefaultDir();
                                SaveDirTraverse();
                                OpenFolder();
                                UnloadContent();
                                bHasLoaded = false;
                            }
                        }
                        break;
                    }
                    #endregion
                    #region Pressing Backspace
                    if (Iinput.KBState.IsKeyDown(Keys.Back) && Iinput.PrevKBState.IsKeyUp(Keys.Back))
                    {
                        /*This allows the user to step out of a current directory.*/
                        iTextOffset = 0;
                        iScrollbarOffset = 0;
                        PrevAS = AS;
                        UnloadContent();
                        bHasLoaded = false;
                        /*This moves you one back in the stack, but checks first to
                         * see if there's only one directory in the stack. If there is,
                         * then you don't move back (most likely, you're at the
                         * root directory, C:\.*/
                        if (DirStack.Count != 1)
                        {
                            DirStack.RemoveAt(DirStack.Count - 1);
                            sSelectedDir = DirStack[DirStack.Count - 1];
                        }
                        break;
                    }
                    #endregion

                    CheckUDInput(gameTime, ScrollUp_Down);                    

                    #region Pressing Left
                    if (Iinput.KBState.IsKeyDown(Keys.Left) && Iinput.PrevKBState.IsKeyUp(Keys.Left))
                    {
                        iFolderDecision = 0;
                        break;
                    }
                    #endregion
                    #region Pressing Right
                    if (Iinput.KBState.IsKeyDown(Keys.Right) && Iinput.PrevKBState.IsKeyUp(Keys.Right))
                    {
                        iFolderDecision = 1;
                        break;
                    }
                    #endregion

                    break;
                #endregion
                #region Graphics Selection
                case AnimStates.GraphicsSelection:
                    while (!bHasLoaded)
                    {
                        LoadContent();
                        bHasLoaded = true;
                    }
                    #region draw junk
                    spriteBatch.Begin();
                    GraphicsDevice.Clear(Color.Blue);
                    spriteBatch.DrawString(font, "Graphics Selection: Choose your file to animate.", Vector2.Zero, TextColor);
                    foreach (FilenameText FT in FilenameList)
                    {
                        FT.PosY = ((iTEXT_SPACING * FT.ListPosition) + iTextOffset + iHEADER_SPACING);
                    }
                    foreach (FilenameText FT in FilenameList)
                    {
                        if (ViewportRect.Contains(new Point((int)FT.PosX, (int)FT.PosY)))
                            spriteBatch.DrawString(font, FT.Text, new Vector2(FT.PosX, FT.PosY), TextColor);
                    }

                    if (FilenameList.Count != 0)
                        spriteBatch.Draw(SelectionBar, new Vector2(0, FilenameList[iScrollbarOffset].PosY + 4
                            /*don't ask me why...the 4 just works...*/), Color.White);
                    spriteBatch.End();
                    #endregion
                    #region Pressing Enter
                    if (Iinput.KBState.IsKeyDown(Keys.Enter) && Iinput.PrevKBState.IsKeyUp(Keys.Enter))
                    {
                        if (FilenameList.Count != 0)
                        {
                            iTextOffset = 0;
                            iScrollbarOffset = 0;
                            PrevAS = AS;
                            AS = AnimStates.LoadingElems;
                            bHasLoaded = false;
                        }
                    }
                    #endregion
                    #region Pressing Backspace
                    if (Iinput.KBState.IsKeyDown(Keys.Back) && Iinput.PrevKBState.IsKeyUp(Keys.Back))
                    {
                        PrevAS = AS;
                        //go back to FolderSelection state
                        CycleStatesLeft();
                        /*This moves you back one in the directory traversal list. At this
                         * point, no checking is necessary, for at least two directories
                         * are on the stack.*/
                        DirStack.RemoveAt(DirStack.Count - 1);
                        sSelectedDir = DirStack[DirStack.Count - 1];
                        UnloadContent();
                        bHasLoaded = false;
                    }
                    #endregion

                    CheckUDInput(gameTime, ScrollUp_Down);
                    
                    break;
                #endregion
                #region Loading Elem
                case AnimStates.LoadingElems:
                    switch (PrevAS)
                    {
                        case AnimStates.GraphicsSelection:
                            OpenGraphic();
                            LoadContent();
                            UnloadContent();
                            CycleStatesRight();
                            iSelection = 0;
                            break;
                        case AnimStates.FolderSelection:
                            OpenFolder();
                            UnloadContent();
                            AS = AnimStates.FolderSelection;
                            iSelection = 0;
                            DirStack.Add(sSelectedDir);
                            break;
                    }
                    break;
                #endregion
                #region Animation Modulation
                case AnimStates.AnimationModding:
                    if (Iinput.KBState.IsKeyDown(Keys.F5) && Iinput.PrevKBState.IsKeyUp(Keys.F5))//Reset Button
                    {
                        PrevAS = AS;
                        AS = AnimStates.LoadingElems;
                        LoadContent();
                        AS = AnimStates.AnimationModding;
                    }
                    Animate(gameTime);
                    HandleColorChanging();

                    BG_Color.R = red;
                    BG_Color.G = green;
                    BG_Color.B = blue;
                    TextColor.B = green;
                    TextColor.G = red;
                    TextColor.R = blue;

                    UpdateViewRect(ref AnimRect);
                    UpdateGlobalViewRect(ref GlobalViewRect);
                    spriteBatch.Begin(SpriteSortMode.BackToFront, null, SamplerState.PointClamp, DepthStencilState.None, null);
                    GraphicsDevice.Clear(Color.Blue);
                    spriteBatch.DrawString(font, "Animation Modulation", Vector2.Zero, TextColor);
                    spriteBatch.DrawString(font, "Focus: " + Focus.ToString(), new Vector2(0, 20), TextColor);

                    //Insert bordered ColorRect here. I know, you don't have a border. Work it out!
                    //Draw this on the back layer to ensure it draws before the actual picture.
                    //This will change colors to reveal "floating pixels"
                    spriteBatch.Draw(WhiteBG, new Vector2(SCRN_WIDTH / 4, iHEADER_SPACING),
                        AnimRect, BG_Color, 0, Vector2.Zero, fPicScale, SpriteEffects.None, 1);
                    //the actual picture
                    spriteBatch.Draw(SelectedPic, new Vector2(SCRN_WIDTH / 4, iHEADER_SPACING),
                        AnimRect, Color.White, 0, Vector2.Zero, fPicScale, SpriteEffects.None, 0);
                    spriteBatch.Draw(SelectedPic, new Vector2(0, (SCRN_HEIGHT / 2) + (SCRN_HEIGHT / 4)),
                        GlobalViewRect, Color.White);
                    spriteBatch.Draw(UpArrow, new Vector2(44, 50), Color.White);
                    spriteBatch.Draw(DownArrow, new Vector2(44, 98), Color.White);

                    #region Presssing Enter
                    if (Iinput.KBState.IsKeyDown(Keys.Enter) && Iinput.PrevKBState.IsKeyUp(Keys.Enter))
                    {
                        TextColor = Color.White; //Reset the color for the FolderSelection state.
                        CycleStatesRight();
                        /*This moves you back one in the directory traversal list. At this
                         * point, no checking is necessary, for at least two directories
                         * are on the stack.*/
                        DirStack.RemoveAt(DirStack.Count - 1);
                        sSelectedDir = DirStack[DirStack.Count - 1];
                    }
                    #endregion
                    #region Pressing Backspace
                    if (Iinput.KBState.IsKeyDown(Keys.Back) && Iinput.PrevKBState.IsKeyUp(Keys.Back))
                    {
                        TextColor = Color.White;//Reset the color for the GraphicsSelection state.
                        PrevAS = AS;
                        CycleStatesLeft();
                    }
                    #endregion
                    #region Focuses
                    switch (Focus)
                    {
                        #region FPS
                        case Focuses.FPS:
                            spriteBatch.DrawString(font, iFPS.ToString(), new Vector2(49, 70), TextColor);

                            CheckUDInput(gameTime, AdjustFPS);

                            if (Iinput.KBState.IsKeyDown(Keys.LeftShift) || Iinput.KBState.IsKeyDown(Keys.RightShift))
                            {
                                if (Iinput.KBState.IsKeyDown(Keys.Tab) && Iinput.PrevKBState.IsKeyUp(Keys.Tab))
                                {
                                    CycleFocusLeft(ref iFocusInc);
                                }
                            }
                            else if (Iinput.KBState.IsKeyDown(Keys.Tab) && Iinput.PrevKBState.IsKeyUp(Keys.Tab))
                            {
                                CycleFocusRight(ref iFocusInc);
                            }

                            break;
                        #endregion
                        #region Frames
                        case Focuses.Frames:
                            spriteBatch.DrawString(font, iFrames.ToString(), new Vector2(49, 70), TextColor);

                            CheckUDInput(gameTime, AdjustFrames);

                            if (Iinput.KBState.IsKeyDown(Keys.LeftShift) || Iinput.KBState.IsKeyDown(Keys.RightShift))
                            {
                                if (Iinput.KBState.IsKeyDown(Keys.Tab) && Iinput.PrevKBState.IsKeyUp(Keys.Tab))
                                {
                                    CycleFocusLeft(ref iFocusInc);
                                }
                            }
                            else if (Iinput.KBState.IsKeyDown(Keys.Tab) && Iinput.PrevKBState.IsKeyUp(Keys.Tab))
                            {
                                CycleFocusRight(ref iFocusInc);
                            }
                            break;
                        #endregion
                        #region Offset
                        case Focuses.Offset:
                            #region Check for Technical offset activation
                            if (bTech_offset_on)
                            {
                                spriteBatch.DrawString(font, "Technical Offset is on.",
                                      new Vector2(0, (graphics.PreferredBackBufferHeight / 2) + (graphics.PreferredBackBufferHeight / 4) - 24), Color.White);
                            }
                            else
                            {
                                spriteBatch.DrawString(font, "Technical Offset is off.",
                                    new Vector2(0, (graphics.PreferredBackBufferHeight / 2) + (graphics.PreferredBackBufferHeight / 4) - 24), Color.White);
                            }
                            #endregion
                            #region Draw Arrows
                            spriteBatch.Draw(LeftArrow, new Vector2(10, 124), Color.White);
                            spriteBatch.Draw(RightArrow, new Vector2(118, 124), Color.White);
                            spriteBatch.DrawString(font, "X: " + Offset.X.ToString(),
                                new Vector2(37, 120), TextColor);

                            spriteBatch.Draw(LeftArrow, new Vector2(10, 152), Color.White);//Technical X arrrow
                            spriteBatch.Draw(RightArrow, new Vector2(118, 152), Color.White);//Technical X arrrow
                            spriteBatch.DrawString(font, "TX: " + iTechnical_offset.X.ToString(),
                                new Vector2(37, 148), TextColor);

                            spriteBatch.Draw(UpArrow, new Vector2(100, 50), Color.White);//Technical Y arrrow
                            spriteBatch.Draw(DownArrow, new Vector2(100, 98), Color.White);//Technical Y arrrow
                            spriteBatch.DrawString(font, "TY: " + iTechnical_offset.Y.ToString(),
                                new Vector2(62, 70), TextColor);

                            spriteBatch.DrawString(font, "Y: " + Offset.Y.ToString(),
                                new Vector2(20, 70), TextColor);
                            #endregion

                            CheckUDInput(gameTime, AdjustVPos);
                            CheckLRInput(gameTime, AdjustHPos);

                            #region Pressing Space
                            if (Iinput.KBState.IsKeyDown(Keys.Space) && Iinput.PrevKBState.IsKeyUp(Keys.Space))
                            {
                                if (bTech_offset_on)
                                    bTech_offset_on = false;
                                else
                                    bTech_offset_on = true;
                            }
                            #endregion
                            if (Iinput.KBState.IsKeyDown(Keys.LeftShift) || Iinput.KBState.IsKeyDown(Keys.RightShift))
                            {
                                if (Iinput.KBState.IsKeyDown(Keys.Tab) && Iinput.PrevKBState.IsKeyUp(Keys.Tab))
                                {
                                    CycleFocusLeft(ref iFocusInc);
                                }
                            }
                            else if (Iinput.KBState.IsKeyDown(Keys.Tab) && Iinput.PrevKBState.IsKeyUp(Keys.Tab))
                            {
                                CycleFocusRight(ref iFocusInc);
                            }
                            break;
                        #endregion
                        #region Size
                        case Focuses.ViewSize:
                            spriteBatch.Draw(LeftArrow, new Vector2(10, 124), Color.White);
                            spriteBatch.Draw(RightArrow, new Vector2(118, 124), Color.White);
                            spriteBatch.DrawString(font, "Y: " + ViewSize.Y.ToString(), new Vector2(12, 70), TextColor);
                            spriteBatch.DrawString(font, "X: " + ViewSize.X.ToString(), new Vector2(37, 120), TextColor);

                            CheckUDInput(gameTime, AdjustVSize);
                            CheckLRInput(gameTime, AdjustHSize);

                            if (Iinput.KBState.IsKeyDown(Keys.LeftShift) || Iinput.KBState.IsKeyDown(Keys.RightShift))
                            {
                                if (Iinput.KBState.IsKeyDown(Keys.Tab) && Iinput.PrevKBState.IsKeyUp(Keys.Tab))
                                {
                                    CycleFocusLeft(ref iFocusInc);
                                }
                            }
                            else if (Iinput.KBState.IsKeyDown(Keys.Tab) && Iinput.PrevKBState.IsKeyUp(Keys.Tab))
                            {
                                CycleFocusRight(ref iFocusInc);
                            }
                            break;
                        #endregion
                        #region Scale
                        case Focuses.Scale:
                            spriteBatch.DrawString(font, fPicScale.ToString(), new Vector2(49, 70), TextColor);

                            CheckUDInput(gameTime, AdjustScale);

                            if (Iinput.KBState.IsKeyDown(Keys.LeftShift) || Iinput.KBState.IsKeyDown(Keys.RightShift))
                            {
                                if (Iinput.KBState.IsKeyDown(Keys.Tab) && Iinput.PrevKBState.IsKeyUp(Keys.Tab))
                                {
                                    CycleFocusLeft(ref iFocusInc);
                                }
                            }
                            else if (Iinput.KBState.IsKeyDown(Keys.Tab) && Iinput.PrevKBState.IsKeyUp(Keys.Tab))
                            {
                                CycleFocusRight(ref iFocusInc);
                            }

                            break;
                        #endregion
                    }
                    #endregion
                    spriteBatch.End();
                    break;
                #endregion
            }
            Iinput.PrevKBState = Iinput.KBState;
        }
        #endregion
        #region CycleStatesRight()
        public void CycleStatesRight()
        {
            iStateInc++;
            switch (iStateInc)
            {
                case 1:
                    AS = AnimStates.FolderSelection;
                    break;
                case 2:
                    AS = AnimStates.GraphicsSelection;
                    break;
                case 3:
                    AS = AnimStates.AnimationModding;
                    break;
                case 4:
                    AS = AnimStates.FolderSelection;
                    iStateInc = 1;
                    break;
            }
        }
        #endregion
        #region CycleStatesLeft()
        public void CycleStatesLeft()
        {
            iStateInc--;
            switch (iStateInc)
            {
                case 0:
                    AS = AnimStates.AnimationModding;
                    iStateInc = 3;
                    break;
                case 1:
                    AS = AnimStates.FolderSelection;
                    break;
                case 2:
                    AS = AnimStates.GraphicsSelection;
                    break;
                case 3:
                    AS = AnimStates.AnimationModding;
                    break;
            }
        }
        #endregion
        #region CycleFocusRight()
        public void CycleFocusRight(ref int IFocusInc)
        {
            IFocusInc++;
            switch (IFocusInc)
            {
                case 1:
                    Focus = Focuses.Frames;
                    break;
                case 2:
                    Focus = Focuses.Offset;
                    break;
                case 3:
                    Focus = Focuses.ViewSize;
                    break;
                case 4:
                    Focus = Focuses.Scale;
                    break;
                case 5:
                    Focus = Focuses.FPS;
                    iFocusInc = 0;
                    break;
                //The special case where you cycle left to FPS, then cycle right. This changes is to Frames rather than repeating FPS.
                case 6:
                    Focus = Focuses.Frames;
                    iFocusInc = 1;
                    break;
            }
        }
        #endregion
        #region CycleFocusLeft()
        public void CycleFocusLeft(ref int IFocusInc)
        {
            IFocusInc--;
            switch (IFocusInc)
            {
                //The special case where you cycle right to FPS, then cycle left. This changes is to Frames rather than repeating FPS.
                case -1:
                    Focus = Focuses.Scale;
                    iFocusInc = 4;
                    break;
                case 0:
                    Focus = Focuses.FPS;
                    iFocusInc = 5;
                    break;
                case 1:
                    Focus = Focuses.Frames;
                    break;
                case 2:
                    Focus = Focuses.Offset;
                    break;
                case 3:
                    Focus = Focuses.ViewSize;
                    break;
                case 4:
                    Focus = Focuses.Scale;
                    break;
            }
        }
        #endregion
        #region Animate()
        public void Animate(GameTime gameTime)
        {
            AnimTime += gameTime.ElapsedGameTime;
            if (AnimTime.TotalMilliseconds >= fFPS)
            {
                AnimTime = new TimeSpan(0);
                iFrameInc++;
                if (iFrameInc >= iFrames)
                {
                    iFrameInc = 0;
                }
            }
        }
        #endregion

        #region AdjustFPS
        public void AdjustFPS(int iOffsetValue, bool Positive)
        {
            //Don't fret! It's only to make the char value uppercase
            if (Positive)
            {
                if (iFPS != iMAX_FPS)
                    iFPS += iOffsetValue;
            }
            else
            {
                if (iFPS != 1)
                    iFPS -= iOffsetValue;
            }
            fFPS = 1000 / iFPS;
        }
        #endregion
        #region AdjustFrames
        public void AdjustFrames(int iOffsetValue, bool Positive)
        {
            if (Positive)
            {
                iFrames += iOffsetValue;
            }
            else
            {
                if ((iFrames - 1) != 0)
                    iFrames -= iOffsetValue;
            }
        }
        #endregion
        #region AdjustHPos
        public void AdjustHPos(int iOffsetValue, bool Positive)
        {
            if (Positive)
            {
                if (bTech_offset_on)
                    iTechnical_offset.X += iOffsetValue;
                else
                    Offset.X += iOffsetValue;
            }
            else
            {
                if (bTech_offset_on)
                    iTechnical_offset.X -= iOffsetValue;
                else
                    Offset.X -= iOffsetValue;
            }
        }
        #endregion
        #region AdjustVPos
        public void AdjustVPos(int iOffsetValue, bool Positive)
        {
            if (Positive)
            {
                if (bTech_offset_on)
                    iTechnical_offset.Y += iOffsetValue;
                else
                    Offset.Y += iOffsetValue;
            }
            else
            {
                if (bTech_offset_on)
                    iTechnical_offset.Y -= iOffsetValue;
                else
                    Offset.Y -= iOffsetValue;
            }
        }
        #endregion
        #region AdjustScale
        public void AdjustScale(int iOffsetValue, bool Positive)
        {
            if (Positive)
            {
                if (fPicScale != fMAX_SCALE)
                    fPicScale += .5f * iOffsetValue;
            }
            else
            {
                if (fPicScale != 1)
                    fPicScale -= .5f * iOffsetValue;
            }
        }
        #endregion
        #region AdjustHSize
        public void AdjustHSize(int iOffsetValue, bool Positive)
        {
            if (Positive)
            {
                ViewSize.X += iOffsetValue;
            }
            else
            {
                ViewSize.X -= iOffsetValue;
            }
        }
        #endregion
        #region AdjustVSize
        public void AdjustVSize(int iOffsetValue, bool Positive)
        {
            if (Positive)
            {
                ViewSize.Y += iOffsetValue;
            }
            else
            {
                ViewSize.Y -= iOffsetValue;
            }
        }
        #endregion
        #region ScrollUp_Down
        public void ScrollUp_Down(int iOffsetValue, bool Positive)
        {
            if (Positive)
            {
                if (FilenameList[iScrollbarOffset].PosY + 4 > SCRN_HEIGHT / 2 ||
                            ViewportRect.Contains(new Point((int)FilenameList[0].PosX, (int)FilenameList[0].PosY)))
                {
                    iScrollbarOffset -= iOffsetValue;
                    iSelection -= iOffsetValue;
                    if (iSelection <= -1)
                    {
                        iScrollbarOffset = FilenameList.Count - 1;
                        iSelection = DirList.Count - 1;
                        if (!ViewportRect.Contains(new Point(
                            (int)FilenameList[FilenameList.Count - 1].PosX, (int)FilenameList[FilenameList.Count - 1].PosY)))
                        {
                            iTextOffset = (-(FilenameList.Count - 1) * iTEXT_SPACING) +
                                SCRN_HEIGHT - (iHEADER_SPACING * 2) - iTEXT_SPACING;
                        }
                    }
                }
                else
                {
                    iScrollbarOffset -= iOffsetValue;
                    iSelection -= iOffsetValue;
                    iTextOffset += iTEXT_SPACING;
                }
            }
            else
            {
                if (FilenameList[iScrollbarOffset].PosY + 4 < SCRN_HEIGHT / 2 ||
                            ViewportRect.Contains(new Point((int)FilenameList[FilenameList.Count - 1].PosX,
                                (int)FilenameList[FilenameList.Count - 1].PosY)))
                {
                    iSelection += iOffsetValue;
                    iScrollbarOffset += iOffsetValue;
                    if (iSelection >= DirList.Count)
                    {
                        iScrollbarOffset = 0;
                        iSelection = 0;
                        iTextOffset = 0;
                    }
                }
                else
                {
                    iScrollbarOffset += iOffsetValue;
                    iSelection += iOffsetValue;
                    iTextOffset -= iTEXT_SPACING;
                }
            }
        }
        #endregion

        //For both of these: Right/UP: Positive, Left/Down:Negative
        public void CheckLRInput(GameTime gameTime, ButtonEventHandler DirMethod)
        {
            /*
             * Used to keep the amount of offset with a timed button press system.
             * As the button is held down longer, the amount of offset increases.
             * One is for horizontal, the other is for vertical.
             */
            int iIncOffset = 1;
            if (Iinput.KBState.IsKeyDown(Keys.Left) && Iinput.PrevKBState.IsKeyUp(Keys.Left))
            {
                DirMethod(iIncOffset, false);
            }
            #region Holding Left Button
            if (Iinput.KBState.IsKeyDown(Keys.Left))
            {
                LeftButtonDownTime += gameTime.ElapsedGameTime;
                if (LeftButtonDownTime.TotalMilliseconds >= (iDELAY * 6))
                {
                    iIncOffset = 1;
                    iTimeSwitch = iFAST_INC;
                }
                else if (LeftButtonDownTime.TotalMilliseconds >= (iDELAY * 3))
                {
                    iIncOffset = 1;
                    iTimeSwitch = iMED_INC;
                }
                else if (LeftButtonDownTime.TotalMilliseconds >= iDELAY)
                {
                    iIncOffset = 1;
                    iTimeSwitch = iSLOW_INC;
                }
                ButtonIncTime += gameTime.ElapsedGameTime;
                if (ButtonIncTime.TotalMilliseconds >= iTimeSwitch)
                {
                    DirMethod(iIncOffset, false);
                    ButtonIncTime = TimeSpan.Zero;
                }
            }
            if (Iinput.KBState.IsKeyUp(Keys.Left) && Iinput.PrevKBState.IsKeyDown(Keys.Left))
            {
                LeftButtonDownTime = TimeSpan.Zero;
                iTimeSwitch = iDELAY;
            }
            #endregion
            if (Iinput.KBState.IsKeyDown(Keys.Right) && Iinput.PrevKBState.IsKeyUp(Keys.Right))
            {
                DirMethod(iIncOffset, true);
            }
            #region Holding Right Button
            if (Iinput.KBState.IsKeyDown(Keys.Right))
            {
                RightButtonDownTime += gameTime.ElapsedGameTime;
                if (RightButtonDownTime.TotalMilliseconds >= (iDELAY * 6))
                {
                    iIncOffset = 1;
                    iTimeSwitch = iFAST_INC;
                }
                else if (RightButtonDownTime.TotalMilliseconds >= (iDELAY * 3))
                {
                    iIncOffset = 1;
                    iTimeSwitch = iMED_INC;

                }
                else if (RightButtonDownTime.TotalMilliseconds >= iDELAY)
                {
                    iIncOffset = 1;
                    iTimeSwitch = iSLOW_INC;
                }
                ButtonIncTime += gameTime.ElapsedGameTime;
                if (ButtonIncTime.TotalMilliseconds >= iTimeSwitch)
                {
                    DirMethod(iIncOffset, true);
                    ButtonIncTime = TimeSpan.Zero;
                }
            }
            if (Iinput.KBState.IsKeyUp(Keys.Right) && Iinput.PrevKBState.IsKeyDown(Keys.Right))
            {
                RightButtonDownTime = TimeSpan.Zero;
                iTimeSwitch = iDELAY;
            }
            #endregion
        }
        public void CheckUDInput(GameTime gameTime, ButtonEventHandler DirMethod)
        {
            /*
             * Used to keep the amount of offset with a timed button press system.
             * As the button is held down longer, the amount of offset increases.
             * One is for horizontal, the other is for vertical.
             */
            int iIncOffset = 1;
            if (Iinput.KBState.IsKeyDown(Keys.Up) && Iinput.PrevKBState.IsKeyUp(Keys.Up))
            {
                DirMethod(iIncOffset, true);
            }
            #region Holding Up Button
            if (Iinput.KBState.IsKeyDown(Keys.Up))
            {
                UpButtonDownTime += gameTime.ElapsedGameTime;
                if (UpButtonDownTime.TotalMilliseconds >= (iDELAY * 6))
                {
                    iTimeSwitch = iFAST_INC;
                }
                else if (UpButtonDownTime.TotalMilliseconds >= (iDELAY * 3))
                {
                    iTimeSwitch = iMED_INC;
                }
                else if (UpButtonDownTime.TotalMilliseconds >= iDELAY)
                {
                    iTimeSwitch = iSLOW_INC;
                }
                ButtonIncTime += gameTime.ElapsedGameTime;
                if (ButtonIncTime.TotalMilliseconds >= iTimeSwitch)
                {
                    DirMethod(iIncOffset, true);
                    ButtonIncTime = TimeSpan.Zero;
                }

            }
            if (Iinput.KBState.IsKeyUp(Keys.Up) && Iinput.PrevKBState.IsKeyDown(Keys.Up))
            {
                UpButtonDownTime = TimeSpan.Zero;
                iTimeSwitch = iDELAY;
            }
            #endregion
            if (Iinput.KBState.IsKeyDown(Keys.Down) && Iinput.PrevKBState.IsKeyUp(Keys.Down))
            {
                DirMethod(iIncOffset, false);
                DownButtonDownTime = TimeSpan.Zero;
            }
            #region Holding Down Button
            if (Iinput.KBState.IsKeyDown(Keys.Down) && Iinput.PrevKBState.IsKeyDown(Keys.Down))
            {
                DownButtonDownTime += gameTime.ElapsedGameTime;
                if (DownButtonDownTime.TotalMilliseconds >= (iDELAY * 6))
                {
                    iTimeSwitch = iFAST_INC;
                }
                else if (DownButtonDownTime.TotalMilliseconds >= (iDELAY * 3))
                {
                    iTimeSwitch = iMED_INC;
                }
                else if (DownButtonDownTime.TotalMilliseconds >= iDELAY)
                {
                    iTimeSwitch = iSLOW_INC;
                }
                ButtonIncTime += gameTime.ElapsedGameTime;
                if (ButtonIncTime.TotalMilliseconds >= iTimeSwitch)
                {
                    DirMethod(iIncOffset, false);
                    ButtonIncTime = TimeSpan.Zero;
                }
            }
            if (Iinput.KBState.IsKeyUp(Keys.Down) && Iinput.PrevKBState.IsKeyDown(Keys.Down))
            {
                DownButtonDownTime = TimeSpan.Zero;
                iTimeSwitch = iDELAY;
            }
            #endregion
        }

        #region UpdateViewRect()
        public void UpdateViewRect(ref Rectangle _viewRect)
        {/*This one updates a viewrect a little differently than the other one. 
          * The first set of parantheses allows for by-size sprite sheet traversal.
          * Ergo, if the size is 50, it will increment the viewRect by denominations
          * of 50. However, this would not suffice for animation. Thus another set
          * of parantheses to handle sprite sheet traversal is required. Hence the
          * second set of paranthesis. The second allows for the offset to be
          * whatever it wants while still only incrementing by the orginal size amount.
          * Now, if one still needs that per-pixel precision, the third element allows for
          * that. If you have turned technical offest on (see AnimationModding for
          * more), you will be able to move the sprite one pixel per button press.
          */
            _viewRect.X = ((int)Offset.X * (int)ViewSize.X) + ((int)ViewSize.X * iFrameInc) + (int)iTechnical_offset.X;
            _viewRect.Y = ((int)Offset.Y * (int)ViewSize.Y)/* + ((int)ViewSize.Y * iFrameInc) */+ (int)iTechnical_offset.Y;
            _viewRect.Width = (int)ViewSize.X;
            _viewRect.Height = (int)ViewSize.Y;
        }
        #endregion
        #region UpdateGlobalViewRect()
        public void UpdateGlobalViewRect(ref Rectangle _viewportRect)
        {
            _viewportRect.X = (int)Offset.X * (int)ViewSize.X + (int)iTechnical_offset.X;
            _viewportRect.Y = (int)Offset.Y * (int)ViewSize.Y + (int)iTechnical_offset.Y;
        }
        #endregion
        #region OpenGraphic()
        /// <summary>
        /// Sets the selected directory as the current directory.
        /// </summary>
        public void OpenGraphic()
        {
            sSelectedPic = DirList[iSelection];
        }
        #endregion
        #region OpenFolder()
        /// <summary>
        /// Sets the selected directory as the current directory.
        /// </summary>
        public void OpenFolder()
        {
            sSelectedDir = DirList[iSelection];
        }
        #endregion
        #region SaveDefaultDir()
        /// <summary>
        /// Saves your current directory as the default.
        /// </summary>
        public void SaveDefaultDir()
        {
            DefaultDirectoryWriter = new StreamWriter(sDefaultDirFile, false);
            sSelectedDir = DirStack[DirStack.Count - 1];
            DefaultDirectoryWriter.Write(sSelectedDir);
            DefaultDirectoryWriter.Close();
            sSelectedDir = DirList[iSelection];
        }
        #endregion
        #region SaveDirTraverse()
        /// <summary>
        /// Saves your directory traversal history for later backwards traversal.
        /// </summary>
        public void SaveDirTraverse()
        {
            StackWriter = new StreamWriter(sStackFile, false);
            foreach (string s in DirStack)
            {
                StackWriter.WriteLine(s);
            }
            StackWriter.Close();
            DirStack.Add(sSelectedDir);
        }
        #endregion
        #region HandleColorChanging()
        public void HandleColorChanging()
        {
            #region for Red
            if (r_to_0)
            {
                red--;
                if (red <= 0)
                    r_to_0 = false;
            }
            else
            {
                red++;
                if (red >= 255)
                    r_to_0 = true;
            }
            #endregion
            #region for Green
            if (g_to_0)
            {
                green--;
                if (green <= 0)
                    g_to_0 = false;
            }
            else
            {
                green++;
                if (green >= 255)
                    g_to_0 = true;
            }
            #endregion
            #region for Blue
            if (b_to_0)
            {
                blue--;
                if (blue <= 0)
                    b_to_0 = false;
            }
            else
            {
                blue++;
                if (blue >= 255)
                    b_to_0 = true;
            }
            #endregion
        }
        #endregion
        #endregion
        // TODO: add in an "are you sure?" screen for set as default button
    }
}