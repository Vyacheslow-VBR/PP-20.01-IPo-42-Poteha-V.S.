using Xunit;

namespace PotehaTests
{
    public class SimpleTests
    {
        [Fact]
        public void Test1_TrueIsTrue()
        {
            Assert.True(true);
        }

        [Fact]
        public void Test2_FalseIsFalse()
        {
            Assert.False(false);
        }

        [Fact]
        public void Test3_OnePlusOneEqualsTwo()
        {
            Assert.Equal(2, 1 + 1);
        }
    }
}