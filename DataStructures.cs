using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML;
using SFML.Graphics;
using SFML.System;
using System.Reflection;

namespace Code
{
    static class DataStructuresVars
    {
        public const float LINE_LENGTH = 100;
        public const float LINE_THICKNESS = 10;
        public const float CIRCLE_RADIUS = 20;
    }

    interface IComposite
    {
        void Draw(RenderWindow app);
        void Update(float dT);
        Vector2f Position { get; set; }
        float MyScale { get; set; }
        float ParentScale { get; set; }
        float Rotation { get; set; }
        Color FillColor { get; set; }
        string Name { get; set; }
    }
    
    class Line : RectangleShape, IComposite
    {
        public Line()
        {
            Origin = new Vector2f(DataStructuresVars.LINE_LENGTH / 2, DataStructuresVars.LINE_THICKNESS / 2);
            Position = new Vector2f(ProgramGlobals.RESOLUTION_X/2, ProgramGlobals.RESOLUTION_Y/2);
            Size = new Vector2f(DataStructuresVars.LINE_LENGTH, DataStructuresVars.LINE_THICKNESS);
            Rotation = 0.0f;
            FillColor = Color.Black;
            transformators = new List<IInterpolation>();
        }

        public Line(string name) : this()
        {
            Name = name;
        }

        public void Draw(RenderWindow app)
        {
            app.Draw(this);
        }

        public void AddInterpolator(IInterpolation transformator)
        {
            transformators.Add(transformator);
        }

        public void Update(float dT)
        {
            for (int i = transformators.Count-1; i > 0; --i)
                if (transformators[i].execute(this, dT))
                    transformators.RemoveAt(i);

        }

        public float MyScale
        {
            get { return scale; }
            set { scale = value;  Scale = new Vector2f(parent_scale_modifier * scale, parent_scale_modifier * scale); }
        }
        private float scale=1;
        public float ParentScale
        {
            get { return parent_scale_modifier; }
            set { parent_scale_modifier = value; Scale = new Vector2f(parent_scale_modifier * scale, parent_scale_modifier * scale); }
        }
        public string Name { get; set; }
        private float parent_scale_modifier=1;
        List<IInterpolation> transformators;
    }

    class Circle : CircleShape, IComposite
    {
        public Circle()
        {
            Origin = new Vector2f(DataStructuresVars.CIRCLE_RADIUS, DataStructuresVars.CIRCLE_RADIUS);
            Position = new Vector2f(ProgramGlobals.RESOLUTION_X/2, ProgramGlobals.RESOLUTION_Y/2);
            Radius = DataStructuresVars.CIRCLE_RADIUS;
            FillColor = Color.Black;
            transformators = new List<IInterpolation>();
        }
        
        public Circle(string name) : this()
        {
            Name = name;
        }

        public void Draw(RenderWindow app)
        {
            app.Draw(this);
        }

        public void AddInterpolator(IInterpolation transformator)
        {
            transformators.Add(transformator);
        }

        public void Update(float dT)
        {
            for (int i = transformators.Count-1; i > 0; --i)
                if (transformators[i].execute(this, dT))
                    transformators.RemoveAt(i);
        }

        public float MyScale
        {
            get { return scale; }
            set { scale = value; Scale = new Vector2f(parent_scale_modifier * scale, parent_scale_modifier * scale); }
        }
        private float scale=1;
        public float ParentScale
        {
            get { return parent_scale_modifier; }
            set { parent_scale_modifier = value; Scale = new Vector2f(parent_scale_modifier * scale, parent_scale_modifier * scale); }
        }
        public string Name { get; set; }
        private float parent_scale_modifier=1;
        List<IInterpolation> transformators;
    }

    class Composite : IComposite
    {
        public Composite()
        {
            elems = new List<IComposite>();
            transformators = new List<IInterpolation>();
        }

        public Composite(string name) : this()
        {
            Name = name;
        }

        public void Draw(RenderWindow app)
        {
            foreach (var elem in elems)
                elem.Draw(app);
        }

        public void AddInterpolator(IInterpolation transformator)
        {
            transformators.Add(transformator);
        }

        public void Update(float dT)
        {
            for (int i = transformators.Count-1; i >= 0; --i)
                if (transformators[i].execute(this, dT))
                    transformators.RemoveAt(i);

        }

        public void Add(IComposite elem)
        {
            elems.Add(elem);
        }

        public void BuildCollection(List<IComposite> givenElems)
        {
            foreach (var elem in givenElems)
                elems.Add(Prototype.CloneObject(elem) as IComposite);
        }
        public List<IComposite> GetCollection()
        {
            return elems;
        }

        public Vector2f Position
        {
            get { return position; }
            set
            {
                foreach (var elem in elems)
                    elem.Position -= position;
                position = value;
                foreach (var elem in elems)
                    elem.Position += value;
            }
        }

        public float Rotation
        {
            get { return rotation; }
            set
            {
                foreach (var elem in elems)
                {
                    // position update
                    elem.Rotation -= rotation;
                }
                rotation = value;
                foreach (var elem in elems)
                {
                    // position update
                    elem.Rotation += value;
                }
            }
        }

        public float MyScale
        {
            get { return scale; }
            set
            {
                scale = value;
                foreach (var elem in elems)
                {
                    // position update
                    elem.ParentScale = value*parent_scale_modifier;
                }
            }
        }
        public float ParentScale
        {
            get { return parent_scale_modifier; }
            set { parent_scale_modifier = value; }
        }

        public Color FillColor
        {
            get { return color; }
            set
            {
                color = value;
                foreach (var elem in elems)
                    elem.FillColor = value;
            }
        }
        public string Name { get; set; }

        private Vector2f position;
        private float rotation;
        private float scale=1;
        private float parent_scale_modifier=1;
        private Color color;
        public List<IComposite> elems;
        List<IInterpolation> transformators;
    }

    interface IInterpolation
    {
        bool execute(IComposite elem, float dT);
    }

    class Move : IInterpolation
    {
        public Move(Vector2f vec, Vector2f startPos, int time)
        {
            velocity_x = vec.X / (float)time;
            velocity_y = vec.Y / (float)time;
            lastDistance = distanceCounter = startPos;
            full_time = time;
            current_time = 0;
        }

        public bool execute(IComposite elem, float dT)
        {
            current_time += dT;
            if (current_time<full_time)
            {
                distanceCounter += new Vector2f(velocity_x * dT, velocity_y * dT);
                diffDistance = distanceCounter - lastDistance;
                elem.Position += diffDistance;
                lastDistance += diffDistance;
                return false;
            }
            else
            {
                distanceCounter += new Vector2f(velocity_x * (dT - current_time + full_time), velocity_y * (dT - current_time + full_time));
                diffDistance = distanceCounter - lastDistance;
                elem.Position += diffDistance;
                return true;
            }
        }

        float velocity_x, velocity_y;
        Vector2f distanceCounter;
        Vector2f lastDistance, diffDistance;
        float full_time, current_time;
    }

    class Rotate : IInterpolation
    {
        public Rotate(float degrees, int time)
        {
            velocity = degrees / (float)time;
            full_time = time;
            current_time = 0;
        }

        public bool execute(IComposite elem, float dT)
        {
            current_time += dT;
            if (current_time < full_time)
            {
                elem.Rotation += velocity*dT;
                return false;
            }
            else
            {
                elem.Rotation += velocity*(dT+current_time-full_time);
                return true;
            }
        }

        float velocity;
        float full_time, current_time;
    }

    class Scale : IInterpolation
    {
        public Scale(float modifier, float actual_scale, int time)
        {
            velocity = (modifier-1.0f) * actual_scale / (float)time;
            scaleCounter = actual_scale;
            lastScale = actual_scale;
            diffScale = 0;
            full_time = time;
            current_time = 0;
        }

        public bool execute(IComposite elem, float dT)
        {
            current_time += dT;
            if (current_time < full_time)
            {
                scaleCounter += velocity * dT;
                diffScale = scaleCounter - lastScale;
                elem.MyScale += diffScale;
                lastScale += diffScale;
                return false;
            }
            else
            {
                scaleCounter += velocity * (dT + current_time - full_time);
                diffScale = (int)scaleCounter - lastScale;
                elem.MyScale += diffScale;
                return true;
            }
        }

        float velocity;
        float scaleCounter;
        float lastScale, diffScale;
        float full_time, current_time;
    }

    class Stain : IInterpolation
    {
        public Stain(Color dst_col, Color src_col, int time)
        {
            vel_R = (float)(dst_col.R - src_col.R) / (float)time;
            vel_G = (float)(dst_col.G - src_col.G) / (float)time;
            vel_B = (float)(dst_col.B - src_col.B) / (float)time;
            R = G = B = 0;
            lastR = lastG = lastB = diffR = diffG = diffB = 0;
            full_time = time;
            current_time = 0;
        }

        public bool execute(IComposite elem, float dT)
        {
            current_time += dT;
            if (current_time < full_time)
            {
                R += vel_R * dT;
                G += vel_G * dT;
                B += vel_B * dT;
                diffR = (int)R - lastR;
                diffG = (int)G - lastG;
                diffB = (int)B - lastB;
                elem.FillColor = new Color((byte)(elem.FillColor.R + diffR), (byte)(elem.FillColor.G + diffG), (byte)(elem.FillColor.B + diffB), elem.FillColor.A);
                lastR += diffR;
                lastG += diffG;
                lastB += diffB;
                return false;
            }
            else
            {
                R = vel_R * (dT + current_time - full_time);
                G = vel_G * (dT + current_time - full_time);
                B = vel_B * (dT + current_time - full_time);
                diffR = (int)R - lastR;
                diffG = (int)G - lastG;
                diffB = (int)B - lastB;
                elem.FillColor = new Color((byte)(elem.FillColor.R + (int)R), (byte)(elem.FillColor.G + (int)G), (byte)(elem.FillColor.B + (int)B), elem.FillColor.A);
                return true;
            }
        }

        float vel_R, vel_G, vel_B;
        float R, G, B;
        int lastR, diffR, lastG, diffG, lastB, diffB;
        float full_time, current_time;
    }

    class Show : IInterpolation
    {
        public Show(byte dst_val, byte actual_val, int time)
        {
            velocity = (float)(dst_val - actual_val) / (float)time;
            A = lastA = diffA = 0;
            full_time = time;
            current_time = 0;
        }

        public bool execute(IComposite elem, float dT)
        {
            current_time += dT;
            if (current_time < full_time)
            {
                A += velocity * dT;
                diffA = (int)A - lastA;
                elem.FillColor = new Color(elem.FillColor.R, elem.FillColor.G, elem.FillColor.B, (byte)(elem.FillColor.A + diffA));
                lastA += diffA;
                return false;
            }
            else
            {
                A += velocity * (dT + current_time - full_time);
                diffA = (int)A - lastA;
                elem.FillColor = new Color(elem.FillColor.R, elem.FillColor.G, elem.FillColor.B, (byte)(elem.FillColor.A + diffA));
                return true;
            }
        }

        float velocity;
        float A;
        int lastA, diffA;
        float full_time, current_time;
    }

    public class Prototype

    {
        public static object CloneObject(object objSource)

        {
            Type typeSource = objSource.GetType();
            object objTarget = Activator.CreateInstance(typeSource);
            
            PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (PropertyInfo property in propertyInfo)
            {
                if (property.CanWrite)
                {
                    if ((property.PropertyType.IsValueType && !property.PropertyType.FullName.Contains("Ptr")) || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    {
                        property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    }
                    else if (property.PropertyType.FullName.Contains("Ptr"))
                    {
                        // Handling SFMLNet wrappers for pointers
                    }
                    else
                    {
                        object objPropertyValue = property.GetValue(objSource, null);
                        if (objPropertyValue == null)
                        {
                            property.SetValue(objTarget, null, null);
                        }
                        else
                        {
                            property.SetValue(objTarget, CloneObject(objPropertyValue), null);
                        }
                    }
                }
            }
            if (typeSource == typeof(Composite))
                ((Composite)objTarget).BuildCollection(((Composite)objSource).GetCollection());
            return objTarget;
        }
    }
}
