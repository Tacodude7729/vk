﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Vulkan
{
    public static partial class VulkanNative
    {
        private static NativeLibrary s_nativeLib;

        static VulkanNative()
        {
            s_nativeLib = LoadNativeLibrary();
            LoadFunctionPointers();
        }

        private static NativeLibrary LoadNativeLibrary()
        {
            return NativeLibrary.Load(GetNativeLibraryName());
        }

        private static string GetNativeLibraryName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "vulkan-1.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (RuntimeInformation.OSDescription.Contains("Unix"))
                {
                    // Android
                    return "libvulkan.so";
                }
                else
                {
                    // Desktop Linux
                    return "libvulkan.so.1";
                }
            }
#if NET5_0
            else if (OperatingSystem.IsAndroid())
            {
                return "libvulkan.so";
            }
#endif
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "libvulkan.dylib";
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        internal static Exception CreateUnpatchedException()
        {
            return new InvalidOperationException("This command was has not been patched.");
        }

        private unsafe static IntPtr LoadStaticProcAddr(string name)
        {
            IntPtr ptr = s_nativeLib.LoadFunctionPointer(name);
            if (ptr == IntPtr.Zero) return Marshal.GetFunctionPointerForDelegate<Action>(
                () => throw new PlatformNotSupportedException($"Could not find vulkan command '{name}' on the current platform.")
            );
            return ptr;
        }

        internal unsafe static IntPtr LoadInstanceProcAddr(VkInstance instance, string name)
        {
            IntPtr ptr = vkGetInstanceProcAddr(instance, name);
            if (ptr == IntPtr.Zero) throw new InvalidOperationException($"Could not find vulkan instance command '{name}'. Make sure the extension it's from is enabled on the instance.");
            return ptr;
        }

        internal unsafe static IntPtr LoadDeviceProcAddr(VkDevice device, string name)
        {
            IntPtr ptr = vkGetDeviceProcAddr(device, name);
            if (ptr == IntPtr.Zero) throw new InvalidOperationException($"Could not find vulkan device command '{name}'. Make sure the extension it's from is enabled on the device.");
            return ptr;
        }

        public unsafe static IntPtr vkGetInstanceProcAddr(string name)
        {
            return vkGetInstanceProcAddr(new VkInstance(IntPtr.Zero), name);
        }

        public unsafe static IntPtr vkGetInstanceProcAddr(VkInstance instance, string name)
        {
            int byteCount = Encoding.UTF8.GetByteCount(name);
            byte* utf8Ptr = stackalloc byte[byteCount + 1];
            fixed (char* namePtr = name)
            {
                Encoding.UTF8.GetBytes(namePtr, name.Length, utf8Ptr, byteCount);
            }
            utf8Ptr[byteCount] = 0;
            return vkGetInstanceProcAddr(instance, utf8Ptr);
        }

        public unsafe static IntPtr vkGetDeviceProcAddr(VkDevice device, string name)
        {
            int byteCount = Encoding.UTF8.GetByteCount(name);
            byte* utf8Ptr = stackalloc byte[byteCount + 1];
            fixed (char* namePtr = name)
            {
                Encoding.UTF8.GetBytes(namePtr, name.Length, utf8Ptr, byteCount);
            }
            utf8Ptr[byteCount] = 0;
            return vkGetDeviceProcAddr(device, utf8Ptr);
        }
    }
}
