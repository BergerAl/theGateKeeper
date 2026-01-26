using TheGatekeeper.Contracts;

namespace TheGateKeeper.Server
{
    public static class CurrentPlayerList
    {
        public static IEnumerable<RiotUserDtoV1> ConstUserList()
        {
            return [
                new RiotUserDtoV1() {
                    Name = "Knechter",
                    Tag = "EUW"
                },
                new RiotUserDtoV1() {
                    Name = "xVanitySixx",
                    Tag = "EUW"
                },
                new RiotUserDtoV1() {
                    Name = "Major Hefeweizen",
                    Tag = "xdd"
                },
                new RiotUserDtoV1() {
                    Name = "Tabatschko",
                    Tag = "EUW"
                },
                new RiotUserDtoV1() {
                    Name = "Oizo",
                    Tag = "Luxi"
                },
                new RiotUserDtoV1() {
                    Name = "LuXi",
                    Tag = "Oizo"
                },
                new RiotUserDtoV1() {
                    Name = "Blinded Manu",
                    Tag = "3532"
                },
                new RiotUserDtoV1() {
                    Name = "Hobbobbelmobmob",
                    Tag = "EUW"
                },
                new RiotUserDtoV1() {
                    Name = "DönerBoxSchmaus",
                    Tag = "EUW"
                },
                new RiotUserDtoV1() {
                    Name = "Disco Pablobar",
                    Tag = "CW12"
                },
                new RiotUserDtoV1() {
                    Name = "IGazY",
                    Tag = "EUW"
                }
            ];
        }
    }
}
