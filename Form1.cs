using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HideMySource
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable Files (*.exe)|*.exe";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = openFileDialog.FileName;
            }
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Executable Files (*.exe)|*.exe";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtSavePath.Text = saveFileDialog.FileName;
            }
        }

        private void btnObfuscate_Click(object sender, EventArgs e)
        {
            string inputFilePath = txtFilePath.Text;
            string outputFilePath = txtSavePath.Text;

            if (string.IsNullOrEmpty(inputFilePath) || string.IsNullOrEmpty(outputFilePath))
            {
                MessageBox.Show("Please select both input and output file paths.");
                return;
            }

            try
            {
                AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(inputFilePath);

                // Apply obfuscation techniques
                ObfuscateAssembly(assembly);

                // Save the obfuscated assembly to the output path
                assembly.Write(outputFilePath);

                MessageBox.Show("Obfuscation complete! Output saved to: " + outputFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during obfuscation: " + ex.Message);
            }
        }

        // Perform obfuscation techniques
        private void ObfuscateAssembly(AssemblyDefinition assembly)
        {
            Random random = new Random();

            // 1. Name Protection: Rename types, methods, fields, and parameters
            NameProtection(assembly, random);

            // 2. Resource Protection: Encrypt resources (e.g., strings) in the assembly
            ResourceProtection(assembly);

            // 3. Invalid Metadata: Add fake methods to confuse decompilers
            InvalidMetadata(assembly, random);
        }

        // Rename types, methods, fields, parameters to random names
        private void NameProtection(AssemblyDefinition assembly, Random random)
        {
            foreach (var type in assembly.MainModule.Types)
            {
                // Rename type
                type.Name = GetRandomName(random);

                foreach (var method in type.Methods)
                {
                    // Rename methods
                    method.Name = GetRandomName(random);

                    foreach (var param in method.Parameters)
                    {
                        // Rename parameters
                        param.Name = GetRandomName(random);
                    }
                }

                // Rename fields
                foreach (var field in type.Fields)
                {
                    field.Name = GetRandomName(random);
                }
            }
        }

        // Protect resources (e.g., strings, images)
        private void ResourceProtection(AssemblyDefinition assembly)
        {
            foreach (var resource in assembly.MainModule.Resources.OfType<EmbeddedResource>())
            {
                // Check if the resource is a text file or similar
                if (resource.Name.EndsWith(".txt") || resource.Name.EndsWith(".resx"))
                {
                    // Read the resource data
                    byte[] resourceData = ReadResourceData(resource);

                    // Encrypt (Base64 encode) the resource data
                    byte[] encryptedData = EncryptData(resourceData);

                    // Replace the resource with encrypted data
                    ReplaceResource(assembly, resource, encryptedData);
                }
            }
        }

        // Read resource data from an EmbeddedResource
        private byte[] ReadResourceData(EmbeddedResource resource)
        {
            using (Stream resourceStream = resource.GetResourceStream())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    resourceStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        // Encrypt resource data (for simplicity, we use Base64 encoding here)
        private byte[] EncryptData(byte[] data)
        {
            return Encoding.UTF8.GetBytes(Convert.ToBase64String(data));
        }

        // Replace the resource with the encrypted data (using a new resource)
        private void ReplaceResource(AssemblyDefinition assembly, EmbeddedResource oldResource, byte[] encryptedData)
        {
            // Create a new embedded resource with encrypted data
            EmbeddedResource newResource = new EmbeddedResource(oldResource.Name, oldResource.Attributes, new MemoryStream(encryptedData));

            // Remove the old resource
            assembly.MainModule.Resources.Remove(oldResource);

            // Add the new encrypted resource
            assembly.MainModule.Resources.Add(newResource);
        }

        // Add fake methods or fake metadata to confuse decompilers
        private void InvalidMetadata(AssemblyDefinition assembly, Random random)
        {
            // Adding fake methods to types (to confuse decompilers)
            foreach (var type in assembly.MainModule.Types)
            {
                MethodDefinition fakeMethod = new MethodDefinition(
                    GetRandomName(random),
                    MethodAttributes.Public | MethodAttributes.Static,
                    assembly.MainModule.TypeSystem.Void);
                type.Methods.Add(fakeMethod);

                // Adding fake instructions
                var ilProcessor = fakeMethod.Body.GetILProcessor();
                ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
            }
        }

        // Helper function to generate random names
        private string GetRandomName(Random random)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int length = random.Next(5, 12);
            char[] name = new char[length];
            for (int i = 0; i < length; i++)
            {
                name[i] = chars[random.Next(chars.Length)];
            }
            return new string(name);
        }
    }
}


