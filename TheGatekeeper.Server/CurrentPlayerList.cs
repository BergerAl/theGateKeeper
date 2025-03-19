namespace TheGateKeeper.Server
{
    public static class CurrentPlayerList
    {
        public static IEnumerable<RiotUser> ConstUserList()
        {
            return [
                new RiotUser() {
                    name = "Knechter",
                    tag = "EUW"
                },
                new RiotUser() {
                    name = "Major Hefeweizen",
                    tag = "xdd"
                },
                new RiotUser() {
                    name = "Tabatschko",
                    tag = "EUW"
                },
                new RiotUser() {
                    name = "Oizo",
                    tag = "Luxi"
                },
                new RiotUser() {
                    name = "LuXi",
                    tag = "Oizo"
                },
                new RiotUser() {
                    name = "Blinded Manu",
                    tag = "3532"
                },
                new RiotUser() {
                    name = "Hobbobbelmobmob",
                    tag = "EUW"
                },
                new RiotUser() {
                    name = "DönerBoxSchmaus",
                    tag = "EUW"
                }
            ];
        }
    }
}
