﻿using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Tests
{
    interface ITestFloatValue
    {
        float Value { get; set; }
    }

    interface ITestIntValue
    {
        int Value { get; set; }
    }

    class TestFloatValue : ITestFloatValue
    {
        public TestFloatValue(float i)
        {
            Value = i;
        }

        public float Value { get; set; }
    }

    class TestIntValue : ITestIntValue
    {
        public TestIntValue(int i)
        {
            Value = i;
        }

        public int Value { get; set; }
    }

    struct TestEntityViewStruct : IEntityViewComponent
    {
#pragma warning disable 649
        public ITestFloatValue TestFloatValue;
        public ITestIntValue TestIntValue;
#pragma warning restore 649        

        public EGID ID { get; set; }
    }
}
