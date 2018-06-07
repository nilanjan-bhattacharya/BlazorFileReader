﻿using Microsoft.AspNetCore.Blazor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileReaderComponent
{
    public interface IFileReaderRef
    {
        IEnumerable<IFileReference> Files { get; }
    }

    public interface IFileReference
    {

        /// <summary>
        /// Opens a stream to read the file
        /// </summary>
        /// <returns></returns>
        Stream OpenRead();

        /// <summary>
        /// Returns the name of the file referenced by the File object.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Returns the size of the file in bytes.
        /// </summary>
        long? Size { get; }

        /// <summary>
        /// Returns the MIME type of the file.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Returns the last modified time of the file, in millisecond since the UNIX epoch (January 1st, 1970 at Midnight).
        /// </summary>
        long? LastModified { get; }

        DateTime? LastModifiedDate { get; }
    }

    public static class FileReaderReference
    {
        public static IFileReaderRef Create()
        {
            return new FileReaderRef();
        }
    }
    

    internal class FileReaderRef : IFileReaderRef
    {
        public IEnumerable<IFileReference> Files => 
            Enumerable.Range(0, FileReaderJsInterop.GetFileCount(GetElementRef()))
                .Select(index => (IFileReference)new FileReference(this, index));

        public Func<ElementRef> GetElementRef { get; internal set; }

        internal FileReaderRef()
        {
        }
    }

    internal class FileReference : IFileReference
    {
        private readonly static DateTime Epoch = new DateTime(1970, 01, 01);
        private readonly FileReaderRef fileLoaderRef;
        private readonly int index;
        private readonly Lazy<string> name;
        private readonly Lazy<long?> size;
        private readonly Lazy<string> type;
        private readonly Lazy<long?> lastModified;
        private readonly Lazy<DateTime?> lastModifiedDate;

        public FileReference(FileReaderRef fileLoaderRef, int index)
        {
            this.fileLoaderRef = fileLoaderRef;
            this.index = index;
            this.name = new Lazy<string>(() => FromElement("name"));
            this.size = new Lazy<long?>(() =>  FromElementLong("size"));
            this.type = new Lazy<string>(() => FromElement("name"));
            this.lastModified = new Lazy<long?>(() => FromElementLong("lastModified"));
            this.lastModifiedDate = new Lazy<DateTime?>(() => 
                LastModified == null ? null: (DateTime?)Epoch.AddMilliseconds(this.LastModified.Value));
        }

        public string Name => this.name.Value;

        public long? Size => this.size.Value;

        public string Type => this.type.Value;

        public long? LastModified => this.lastModified.Value;

        public DateTime? LastModifiedDate => this.lastModifiedDate.Value;

        public Stream OpenRead()
        {
            return FileReaderJsInterop.OpenFileStream(fileLoaderRef.GetElementRef(), index);
        }

        private string FromElement(string name)
        {
            return FileReaderJsInterop.GetFileProperty(fileLoaderRef.GetElementRef(), index, name);
        }

        private long? FromElementLong(string name)
        {
            var s = FromElement(name);
            return (s == null) ? new long?() : new long?(long.Parse(s));
        }
    }

   
}