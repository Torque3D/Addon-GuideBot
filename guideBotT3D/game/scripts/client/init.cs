//-----------------------------------------------------------------------------
// Torque
// Copyright GarageGames, LLC 2011
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Variables used by client scripts & code.  The ones marked with (c)
// are accessed from code.  Variables preceeded by Pref:: are client
// preferences and stored automatically in the ~/client/prefs.cs file
// in between sessions.
//
//    (c) Client::MissionFile             Mission file name
//    ( ) Client::Password                Password for server join

//    (?) Pref::Player::CurrentFOV
//    (?) Pref::Player::DefaultFov
//    ( ) Pref::Input::KeyboardTurnSpeed

//    (c) pref::Master[n]                 List of master servers
//    (c) pref::Net::RegionMask
//    (c) pref::Client::ServerFavoriteCount
//    (c) pref::Client::ServerFavorite[FavoriteCount]
//    .. Many more prefs... need to finish this off

// Moves, not finished with this either...
//    (c) firstPerson
//    $mv*Action...

//-----------------------------------------------------------------------------
// These are variables used to control the shell scripts and
// can be overriden by mods:
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
function initClient()
{
   echo("\n--------- Initializing " @ $appName @ ": Client Scripts ---------");

   // Make sure this variable reflects the correct state.
   $Server::Dedicated = false;

   // Game information used to query the master server
   $Client::GameTypeQuery = $appName;
   $Client::MissionTypeQuery = "Any";

   exec("art/gui/customProfiles.cs"); // override the base profiles if necessary

   // The common module provides basic client functionality
   initBaseClient();

   // Use our prefs to configure our Canvas/Window
   configureCanvas();

   // Load up the Game GUIs
   exec("art/gui/defaultGameProfiles.cs");
   exec("art/gui/PlayGui.gui");
   exec("art/gui/ChatHud.gui");
   exec("art/gui/playerList.gui");
   exec("art/gui/hudlessGui.gui");
   exec("art/gui/controlsHelpDlg.gui");

   // Load up the shell GUIs
   if($platform !$= "xenon")  // Use the unified shell instead
      exec("art/gui/mainMenuGui.gui");
   exec("art/gui/joinServerDlg.gui");
   exec("art/gui/endGameGui.gui");
   exec("art/gui/StartupGui.gui");

   // Gui scripts
   exec("./playerList.cs");
   exec("./chatHud.cs");
   exec("./messageHud.cs");
   exec("scripts/gui/playGui.cs");
   exec("scripts/gui/startupGui.cs");

   // Client scripts
   exec("./client.cs");
   exec("./game.cs");
   exec("./missionDownload.cs");
   exec("./serverConnection.cs");

   // Load useful Materials
   exec("./shaders.cs");

   // Default player key bindings
   exec("./default.bind.cs");

   if (isFile("./config.cs"))
      exec("./config.cs");

   //.logicking >>
   //exec("~/gui/ShellGui.cs");
   exec("./logickingMechanics/init.cs");
   //.logicking<<
   loadMaterials();

   // Really shouldn't be starting the networking unless we are
   // going to connect to a remote server, or host a multi-player
   // game.
   setNetPort(0);

   // Copy saved script prefs into C++ code.
   setDefaultFov( $pref::Player::defaultFov );
   setZoomSpeed( $pref::Player::zoomSpeed );

   if( isFile( "./audioData.cs" ) )
      exec( "./audioData.cs" );

   // Start up the main menu... this is separated out into a
   // method for easier mod override.

   if ($startWorldEditor || $startGUIEditor) {
      // Editor GUI's will start up in the primary main.cs once
      // engine is initialized.
      return;
   }

   // Connect to server if requested.
   if ($JoinGameAddress !$= "") {
      // If we are instantly connecting to an address, load the
      // loading GUI then attempt the connect.
      loadLoadingGui();
      connect($JoinGameAddress, "", $Pref::Player::Name);
   }
   else {
      // Otherwise go to the splash screen.
      Canvas.setCursor("DefaultCursor");
      loadStartup();
   }   
}


//-----------------------------------------------------------------------------
function loadDefaultMission(%type)
{
   %type = %type !$="" ? %type : "SinglePlayer";
   Canvas.setCursor("DefaultCursor");
   //createServer( %type, "levels/simple.mis" );
   createServer( %type, "levels/guideBotDemo.mis" );

   %conn = new GameConnection(ServerConnection);
   RootGroup.add(ServerConnection);
   %conn.setConnectArgs($pref::Player::Name);
   %conn.setJoinPassword($Client::Password);
   %conn.connectLocal();
}

function loadMainMenu()
{
   // Startup the client with the Main menu...
   if (isObject( MainMenuGui ))
      Canvas.setContent( MainMenuGui );
   else if (isObject( UnifiedMainMenuGui ))
      Canvas.setContent( UnifiedMainMenuGui );
   Canvas.setCursor("DefaultCursor");

   // first check if we have a level file to load
   if ($levelToLoad !$= "")
   {
      %levelFile = "levels/";
      %ext = getSubStr($levelToLoad, strlen($levelToLoad) - 3, 3);
      if(%ext !$= "mis")
         %levelFile = %levelFile @ $levelToLoad @ ".mis";
      else
         %levelFile = %levelFile @ $levelToLoad;

      // Clear out the $levelToLoad so we don't attempt to load the level again
      // later on.
      $levelToLoad = "";
      
      // let's make sure the file exists
      %file = findFirstFile(%levelFile);

      if(%file !$= "")
         createAndConnectToLocalServer( "SinglePlayer", %file );
   }
}

function loadLoadingGui(%displayText)
{
   Canvas.setContent("LoadingGui");
   LoadingProgress.setValue(1);

   if (%displayText !$= "")
   {
      LoadingProgressTxt.setValue(%displayText);
   }
   else
   {
      LoadingProgressTxt.setValue("WAITING FOR SERVER");
   }

   Canvas.repaint();
}
