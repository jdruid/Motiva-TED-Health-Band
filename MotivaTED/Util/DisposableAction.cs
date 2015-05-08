﻿// Copyright (c) Microsoft Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

namespace MotivaTED
{
    public class DisposableAction : IDisposable
    {
        private Action dispose;

        public DisposableAction(Action dispose)
        {
            if (dispose == null)
            {
                throw new ArgumentNullException("dispose");
            }

            this.dispose = dispose;
        }

        public DisposableAction(Action construct, Action dispose)
        {
            if (construct == null)
            {
                throw new ArgumentNullException("construct");
            }

            if (dispose == null)
            {
                throw new ArgumentNullException("dispose");
            }

            construct();

            this.dispose = dispose;
        }

        public void Dispose()
        {
            if (dispose != null)
            {
                try
                {
                    dispose();
                }
                catch { }

                dispose = null;
            }
        }
    }
}

