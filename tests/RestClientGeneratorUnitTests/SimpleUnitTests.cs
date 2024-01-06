namespace RestClientGeneratorUnitTests;

[TestClass]
public class SimpleUnitTests
{
    [TestMethod]
    public void TestMethod1()
    {
        var client = new TestRestClientContext().GetSimpleTestClient();
    }
}