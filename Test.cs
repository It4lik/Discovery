namespace discovery
{
    public class Test
    {
        private string testStr;
        private char[] testArray;

        public Test(string test) {
            testStr = test;
            testArray = testStr.ToCharArray(0,3);
            System.Console.WriteLine(testArray[0]);
            System.Console.WriteLine(testArray[1]);
        }
    }
}