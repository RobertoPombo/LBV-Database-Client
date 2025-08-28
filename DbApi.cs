namespace LBV_Database_Client
{
    public class DbApi
    {
        public static readonly DbApiConnectionConfig Con;

        public static DbApiConnectionConfig DynCon { get { return DbApiConnectionConfig.GetActiveConnection() ?? DbApiConnectionConfig.List[0]; } }

        static DbApi()
        {
            DbApiConnectionConfig.LoadJson();
            Con = DbApiConnectionConfig.GetActiveConnection() ?? DbApiConnectionConfig.List[0];
        }
    }
}
