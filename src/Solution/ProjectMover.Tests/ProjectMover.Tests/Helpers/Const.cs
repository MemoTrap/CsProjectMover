#pragma warning disable IDE0130
#pragma warning disable IDE1006

namespace ProjectMover.Tests {
  internal static class Const {
    public const string TEST_FIELDS = "TestFields";
    public const string TEST_FIELD_1 = "ProjectMover.TestField1";
    public const string TEST_FIELD_2 = "ProjectMover.TestField2";
    public const string TEST_FIELD_3 = "ProjectMover.TestField3";
    
    public const string A_LIB_CORE = "ALibCore";
    public const string B_LIB_UTIL = "BLibUtil";
    public const string C_LIB_APP = "CLibApp";
    public const string D_LIB_PLUGIN = "DLibPlugin";
    public const string E_LIB_WITH_LINKS = "ELibWithLinks";
    public const string F_SHARED_APP = "FSharedApp";
    public const string G_MIXED_APP = "GMixedApp";
    public const string S_SHARED_STUFF = "SSharedStuff";
    public const string Z_LIB_STD = "ZLibStd";

    public const string PROJ_A_LIB_CORE = A_LIB_CORE + CSPROJ;
    public const string PROJ_B_LIB_UTIL = B_LIB_UTIL + CSPROJ;
    public const string PROJ_C_LIB_APP = C_LIB_APP + CSPROJ;
    public const string PROJ_D_LIB_PLUGIN = D_LIB_PLUGIN + CSPROJ;
    public const string PROJ_E_LIB_WITH_LINKS = E_LIB_WITH_LINKS + CSPROJ;
    public const string PROJ_F_SHARED_APP = F_SHARED_APP + CSPROJ;
    public const string PROJ_G_MIXED_APP = G_MIXED_APP + CSPROJ;
    public const string PROJ_S_SHARED_STUFF = S_SHARED_STUFF + SHPROJ;
    public const string PROJ_Z_LIB_STD = Z_LIB_STD + CSPROJ;


    public const string SLN_SOL_APP = "SolApp" + SLN;
    public const string SLN_SOL_CORE = "SolCore" + SLN;
    public const string SLN_SOL_LINKS = "SolLinks" + SLN;
    public const string SLN_SOL_MIXED = "SolMixed" + SLN;
    public const string SLN_SOL_PLUGIN = "SolPlugin" + SLN;
    public const string SLN_SOL_SHARED = "SolShared" + SLN;

    public const string RELOCATED = "Relocated";
    public const string COPY = ".Copy";

    public const string CSPROJ = ".csproj";
    public const string SHPROJ = ".shproj";
    public const string PROJITEMS = ".projitems";
    public const string WC_CSPROJ = "*" + CSPROJ;
    public const string WC_SHPROJ = "*" + SHPROJ;
    public const string WC_PROJITEMS = "*" + PROJITEMS;
    public const string SLN = ".sln";

    public const string PROJECT_MOVER_SLN = "ProjectMover.sln";
    public const string TESTFIELDS_TEMPSVN_PLAYGROUND = @"TestFields\Temp-svn\Playground";

    public const string WILDCARD = "*";

    public const string RPC_APP_TEMPLATE = "RpcAppTemplate";
    public const string APPLICATION = "Application";
    public const string CLIENT = "Client";
    public const string COMMON = "Common";
    public const string SERVER = "Server";
    
    public const string SLN_APPLICATION = APPLICATION + SLN;

    public const string PROJ_CLIENT_CONSOLE_APP = "ClientConsoleApp" + CSPROJ;
    public const string PROJ_CLIENT_GUI_APP = "ClientGuiApp" + CSPROJ;
    public const string PROJ_CLIENT_LIB = "ClientLib" + CSPROJ;
    public const string PROJ_CLIENT_PROXY = "ClientProxy" + CSPROJ;
    public const string PROJ_COMMON_BCL = "CommonBcl" + CSPROJ;
    public const string PROJ_COMMON_CONTRACT = "CommonContract" + CSPROJ;
    public const string PROJ_SERVER_APP = "ServerApp" + CSPROJ;
    public const string PROJ_SERVER_BIZ = "ServerBiz" + CSPROJ;
    public const string PROJ_SERVER_HOST = "ServerHost" + CSPROJ;
    public const string PROJ_SERVER_SERVICE = "ServerService" + CSPROJ;

    public const string PROJ_RPC_CLIENT = "RpcClient" + CSPROJ;
    public const string PROJ_RPC_CLIENT_PROXY_GENERATOR = "RpcClientProxyGenerator" + CSPROJ;
    public const string PROJ_RPC_CLIENT_WINFORMS_CONTROLS = "RpcClientWinFormsControls" + CSPROJ;
    public const string PROJ_RPC_COMMON = "RpcCommon" + CSPROJ;
    public const string PROJ_RPC_SERVER = "RpcServer" + CSPROJ;


    public const string SLN_NET_FRAMEWORK_1 = "NetFramework1" + SLN;
    public const string SLN_NET_FRAMEWORK_2 = "NetFramework2" + SLN;

    public const string CLASS_LIBRARY_1 = "ClassLibrary1";
    public const string CLASS_LIBRARY_2 = "ClassLibrary2";
    public const string CLASS_LIBRARY_3 = "ClassLibrary3";
                        
    public const string CONSOLE_APP_1 = "ConsoleApp1";
    public const string CONSOLE_APP_2 = "ConsoleApp2";
    public const string CONSOLE_APP_3 = "ConsoleApp3";
    
    public const string SHARED_PROJECT_1 = "SharedProject1";

    public const string PROJ_CLASS_LIBRARY_1 = CLASS_LIBRARY_1 + CSPROJ;
    public const string PROJ_CLASS_LIBRARY_2 = CLASS_LIBRARY_2 + CSPROJ;
    public const string PROJ_CLASS_LIBRARY_3 = CLASS_LIBRARY_3 + CSPROJ;
                        
    public const string PROJ_CONSOLE_APP_1 = CONSOLE_APP_1 + CSPROJ;
    public const string PROJ_CONSOLE_APP_2 = CONSOLE_APP_2 + CSPROJ;
    public const string PROJ_CONSOLE_APP_3 = CONSOLE_APP_3 + CSPROJ;
                        
    public const string PROJ_SHARED_PROJECT_1 = SHARED_PROJECT_1 + SHPROJ;
  }
}
