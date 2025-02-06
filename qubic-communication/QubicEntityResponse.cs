using System;
using System.Text;
using System.Threading.Tasks;

/*

Permission is hereby granted, perpetual, worldwide, non-exclusive, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
and to permit persons to whom the Software is furnished to do so, subject to the following conditions:


  1. The Software cannot be used in any form or in any substantial portions for development, maintenance and for any other purposes, in the military sphere and in relation to military products, 
  including, but not limited to:

    a. any kind of armored force vehicles, missile weapons, warships, artillery weapons, air military vehicles (including military aircrafts, combat helicopters, military drones aircrafts), 
    air defense systems, rifle armaments, small arms, firearms and side arms, melee weapons, chemical weapons, weapons of mass destruction;

    b. any special software for development technical documentation for military purposes;

    c. any special equipment for tests of prototypes of any subjects with military purpose of use;

    d. any means of protection for conduction of acts of a military nature;

    e. any software or hardware for determining strategies, reconnaissance, troop positioning, conducting military actions, conducting special operations;

    f. any dual-use products with possibility to use the product in military purposes;

    g. any other products, software or services connected to military activities;

    h. any auxiliary means related to abovementioned spheres and products.


  2. The Software cannot be used as described herein in any connection to the military activities. A person, a company, or any other entity, which wants to use the Software, 
  shall take all reasonable actions to make sure that the purpose of use of the Software cannot be possibly connected to military purposes.


  3. The Software cannot be used by a person, a company, or any other entity, activities of which are connected to military sphere in any means. If a person, a company, or any other entity, 
  during the period of time for the usage of Software, would engage in activities, connected to military purposes, such person, company, or any other entity shall immediately stop the usage 
  of Software and any its modifications or alterations.


  4. Abovementioned restrictions should apply to all modification, alteration, merge, and to other actions, related to the Software, regardless of how the Software was changed due to the 
  abovementioned actions.


The above copyright notice and this permission notice shall be included in all copies or substantial portions, modifications and alterations of the Software.


THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

public class QubicEntityResponse : IQubicBuildPackage
{
    private const int _internalPackageSize = 840;

    private QubicEntity entity = new QubicEntity();
    private int tick = 0;
    private int spectrumIndex = 0;
    private byte[] siblings = new byte[0];

    /* Gets the entity */
    public QubicEntity GetEntity()
    {
        return entity;
    }

    /* Sets the entity */
    public void SetEntity(QubicEntity entity)
    {
        this.entity = entity;
    }

    /* Gets the tick */
    public int GetTick()
    {
        return tick;
    }

    /* Sets the tick */
    public void SetTick(int tick)
    {
        this.tick = tick;
    }

    /* Gets the spectrum index */
    public int GetSpectrumIndex()
    {
        return spectrumIndex;
    }

    /* Sets the spectrum index */
    public void SetSpectrumIndex(int spectrumIndex)
    {
        this.spectrumIndex = spectrumIndex;
    }

    /* Gets the siblings */
    public byte[] GetSiblings()
    {
        return siblings;
    }

    /* Sets the siblings */
    public void SetSiblings(byte[] siblings)
    {
        this.siblings = siblings;
    }

    /* Constructor */
    public QubicEntityResponse()
    {
    }

    /* Gets the package size */
    public int GetPackageSize()
    {
        return GetPackageData().Length;
    }

    /* Parses the data */
    public QubicEntityResponse Parse(byte[] data)
    {
        if (data.Length != _internalPackageSize)
        {
            Console.Error.WriteLine("INVALID PACKAGE SIZE");
            return null;
        }
        var dataView = new DataView(data);
        int offset = 0;
        var entity = new QubicEntity();
        if (entity.Parse(data.AsSpan(0, entity.GetPackageSize()).ToArray()) != null)
        {
            SetEntity(entity);
            offset += entity.GetPackageSize();

            SetTick(dataView.GetInt32(offset, true));
            offset += 4;
            SetSpectrumIndex(dataView.GetInt32(offset, true));
            offset += 4;
            SetSiblings(data.AsSpan(offset).ToArray());
        }
        return this;
    }

    /* Gets the package data */
    public byte[] GetPackageData()
    {
        var builder = new QubicPackageBuilder(_internalPackageSize);
        builder.Add(entity);
        builder.AddInt(tick);
        builder.AddInt(spectrumIndex);
        builder.AddRaw(siblings);
        return builder.GetData();
    }
}

