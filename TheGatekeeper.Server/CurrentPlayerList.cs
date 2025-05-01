namespace TheGateKeeper.Server
{
    public static class CurrentPlayerList
    {
        public static IEnumerable<RiotUser> ConstUserList()
        {
            return [
                new RiotUser() {
                    Name = "Knechter",
                    Tag = "EUW"
                },
                new RiotUser() {
                    Name = "Major Hefeweizen",
                    Tag = "xdd"
                },
                new RiotUser() {
                    Name = "Tabatschko",
                    Tag = "EUW"
                },
                new RiotUser() {
                    Name = "Oizo",
                    Tag = "Luxi"
                },
                new RiotUser() {
                    Name = "LuXi",
                    Tag = "Oizo"
                },
                new RiotUser() {
                    Name = "Blinded Manu",
                    Tag = "3532"
                },
                new RiotUser() {
                    Name = "Hobbobbelmobmob",
                    Tag = "EUW"
                },
                new RiotUser() {
                    Name = "DönerBoxSchmaus",
                    Tag = "EUW"
                }
            ];
        }
    }
}
