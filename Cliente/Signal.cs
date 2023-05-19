using System;
using System.Collections.Generic;
using System.Text;

namespace Cliente
{
    class Signal
    {
        public string User { get; set; }
        public double Gain { get; set; }
        public string ImageModel { get; set; }

        public Signal(string user, double gain, string imageModel)
        {
            User = user;
            Gain = gain;
            ImageModel = imageModel;
        }

        public override string ToString()
        {
            return $"User: {User}, Gain: {Gain}, ImageModel: {ImageModel}";
        }
    }
}
