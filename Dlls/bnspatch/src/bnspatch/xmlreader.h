#pragma once
#include <cstdint>

namespace v13
{
    class XmlReaderIO
    {
    public:
        enum ErrCode
        {
            ERR_NO_ERROR,
            ERR_UNKNOWN,
            ERR_SYSTEM,
            ERR_NO_MORE_FILE,
            ERR_NOT_IMPLEMENTED,
            ERR_INVALID_PARAM,
            ERR_INSUFFICIENT_BUFFER
        };

        virtual enum ErrCode Open(const wchar_t* path, const wchar_t* xml, bool recursive) = 0;
        virtual enum ErrCode Read(unsigned char* buf, unsigned int* bufsize) const = 0;
        virtual unsigned int GetFileSize() const = 0;
        virtual const wchar_t* GetFileName() const = 0;
        virtual enum ErrCode Next() = 0;
        virtual void Close() = 0;
    }; /* size: 0x0008 */

    class XmlReaderLog
    {
    public:
        virtual void Error(const wchar_t* format, ...) const = 0;
        virtual void Debug(const wchar_t* format, ...) const = 0;
        virtual void Trace(const wchar_t* format, ...) const = 0;
    }; /* size = 0x0008 */

    class XmlDoc
    {
    public:
        virtual bool IsValid() const = 0;
        virtual const wchar_t* Name() const = 0;
        virtual class XmlElement* Root() = 0;
        virtual int BinarySize() const = 0;
        virtual void SerializeTo(char* buf, int size) const = 0;
        virtual void SerializeFrom(char* buf, int size) = 0;
    }; /* size: 0x0008 */

    class XmlNode
    {
    public:
        enum TYPE
        {
            XML_NONE,
            XML_ELEMENT,
            XML_TEXT,
        };

        virtual enum TYPE Type() const = 0;
        virtual bool IsValid() const = 0;
        virtual const wchar_t* Name() const = 0;
        virtual class XmlDoc const* GetDoc() const = 0;
        virtual class XmlNode const* Parent() const = 0;
        virtual int ChildCount() const = 0;
        virtual const class XmlNode* FirstChild() const = 0;
        virtual const class XmlNode* Child(int) const = 0;
        virtual const class XmlNode* Next() const = 0;
        virtual long LineNumber() const = 0;
        virtual const wchar_t* GetURI() const = 0;
        virtual int MemSize() const = 0;
        virtual int Clone(char* buf, int size) const = 0;
        virtual class XmlNode* CloneNode(char* buf, int size) const = 0;
        virtual int BinarySize() const = 0;
        virtual void SerializeTo(char*& buf, int& size) const = 0;
        virtual void SerializeFrom(char*& buf, int& size) = 0;
        virtual const class XmlElement* ToXmlElement() const = 0;
        virtual class XmlElement* ToXmlElement() = 0;
        virtual const class XmlTextNode* ToXmlTextNode() const = 0;
        virtual class XmlTextNode* ToXmlTextNode() = 0;
    }; /* size: 0x0008 */

    class XmlElement
    {
    public:
        virtual int ChildElementCount() const = 0;
        virtual const class XmlElement* FirstChildElement() const = 0;
        virtual const class XmlElement* NextElement() const = 0;
        virtual const wchar_t* Name() const = 0;
        virtual long LineNumber() const = 0;
        virtual int AttributeCount() const = 0;
        virtual const wchar_t* Attribute(unsigned int nameHash, const wchar_t* name) const = 0;
        virtual const wchar_t* Attribute(int index) const = 0;
        virtual const wchar_t* Attribute(const wchar_t* name) const = 0;
        virtual const wchar_t* AttributeName(int index) const = 0;
        virtual int AttributeIndex(const wchar_t* name) const = 0;
        virtual const class XmlNode* ToXmlNode() const = 0;
        virtual class XmlNode* ToXmlNode() = 0;
    }; /* size: 0x0008 */

    class _XmlSaxHandler
    {
    public:
        virtual bool StartParser() = 0;
        virtual bool EndParser() = 0;
        virtual bool StartElement(class XmlElement*) = 0;
        virtual bool EndElement(class XmlElement*) = 0;
    };

    class XmlReader
    {
    public:
        virtual bool Initialize(class XmlReaderIO* io, const class XmlReaderLog* log, bool useExpat) const = 0;
        virtual const class XmlReaderLog* SetLog(const class XmlReaderLog* log) const = 0;
        virtual const class XmlReaderLog* GetLog() const = 0;
        virtual void Cleanup(bool clearMemory) const = 0;
        virtual class XmlReaderIO* GetIO() const = 0;
        virtual bool Read(const wchar_t* xml, class _XmlSaxHandler& handler) const = 0;
        virtual class XmlDoc* Read(const wchar_t* xml) const = 0;
        virtual class XmlDoc* Read(const unsigned char* mem, unsigned int size, const wchar_t* xmlFileNameForLogging) const = 0;
        virtual void Close(class XmlDoc* doc) const = 0;
        virtual class XmlDoc* NewDoc() const = 0;
        virtual bool IsBinary(const wchar_t* xml) const = 0;
        virtual bool IsBinary(const unsigned char* mem, unsigned int size) const = 0;
    }; /* size: 0x0008 */

    struct XmlCfgDef
    {
        enum
        {
            CFG_NONE,
            CFG_INT,
            CFG_INT64,
            CFG_FLOAT,
            CFG_BOOL,
            CFG_IP,
            CFG_STR,
            CFG_DIR,
            CFG_ANSI,
            CFG_GROUP,
            CFG_TYPE_MAX
        };
        enum
        {
            CFG_OPTIONAL,
            CFG_REQUIRED,
            CFG_DERIVED,
            CFG_DEV
        };

        /* 0x0000 */ size_t offset;
        /* 0x0008 */ const wchar_t* name;
        /* 0x0010 */ int32_t type;
        /* 0x0014 */ int32_t reqOption;
        /* 0x0018 */ const void* defaultValue;
        /* 0x0020 */ const wchar_t* desc;
    }; /* size: 0x0028 */

    struct XmlCfgDef2
    {
        enum
        {
            CFG_NONE,
            CFG_INT,
            CFG_INT64,
            CFG_FLOAT,
            CFG_BOOL,
            CFG_IP,
            CFG_STR,
            CFG_DIR,
            CFG_ANSI,
            CFG_GROUP,
            CFG_TYPE_MAX
        };
        enum
        {
            CFG_OPTIONAL,
            CFG_REQUIRED,
            CFG_DERIVED,
            CFG_DEV
        };

        /* 0x0000 */ size_t offset;
        /* 0x0008 */ const wchar_t* name;
        /* 0x0010 */ int32_t type;
        /* 0x0014 */ int32_t reqOption;
        /* 0x0018 */ const void* defaultValue;
        /* 0x0020 */ const wchar_t* desc;
        /* 0x0028 */ const wchar_t* derivedFrom;
        /* 0x0030 */ int32_t firstChildIndex;
        /* 0x0034 */ int32_t nextSiblingIndex;
    }; /* size: 0x0038 */

    class XmlConfigReader
    {
    public:
        virtual bool Initialize(const struct XmlCfgDef2* def2, int cnt, void* (*allocFunc)(size_t), size_t structSize) const = 0;
        virtual bool Initialize(const struct XmlCfgDef* def, int cnt, void* (*allocFunc)(size_t), size_t structSize) const = 0;
        virtual void Cleanup() const = 0;
        virtual void* Read(size_t& bufferSize, unsigned char* configXmlString, unsigned int configXmlStringSize, const wchar_t* filename, const wchar_t* cfgname, bool versionVerification, int configDefinitionMajorVersion, int configDefinitionMinorVersion, bool loadDefaultForUnspecified, bool readOnly) const = 0;
        virtual void* Read(size_t& bufferSize, const wchar_t* filename, const wchar_t* cfgname, bool versionVerification, int configDefinitionMajorVersion, int configDefinitionMinorVersion, bool loadDefaultForUnspecified, bool readOnly) const = 0;
        virtual void* Read(const wchar_t* filename, const wchar_t* cfgname, bool readOnly) const = 0;
        virtual wchar_t const* GetLastErrorString() const = 0;
    }; /* size: 0x0008 */

    class XmlTextNode
    {
    public:
        virtual const wchar_t* Value() const = 0;
        virtual const class XmlNode* ToXmlNode() const = 0;
        virtual class XmlNode* ToXmlNode() = 0;
    }; /* size: 0x0008 */
}

namespace v14
{
    using v13::XmlCfgDef;
    using v13::XmlCfgDef2;
    using v13::XmlConfigReader;
    using v13::XmlDoc;
    using v13::XmlElement;
    using v13::_XmlSaxHandler;
    using v13::XmlNode;
    using v13::XmlReaderIO;
    using v13::XmlReaderLog;
    using v13::XmlTextNode;

    class XmlPieceReader
    {
    public:
        virtual bool Read(class XmlDoc* doc) = 0;
        virtual int GetMaxNodeCountPerPiece() = 0;
    }; /* size: 0x0008 */

    class XmlReader
    {
    public:
        virtual bool Initialize(class XmlReaderIO* io, const class XmlReaderLog* log, bool useExpat) const = 0;
        virtual const class XmlReaderLog* SetLog(const class XmlReaderLog*) const = 0;
        virtual const class XmlReaderLog* GetLog() const = 0;
        virtual void Cleanup(bool) const = 0;
        virtual class XmlReaderIO* GetIO() const = 0;
        virtual bool Read(const wchar_t* xml, class _XmlSaxHandler& handler) const = 0;
        virtual class XmlDoc* Read(const wchar_t* xml, class XmlPieceReader* xmlPieceReader) const = 0;
        virtual class XmlDoc* Read(const unsigned char* mem, unsigned int size, const wchar_t* xmlFileNameForLogging, class XmlPieceReader* xmlPieceReader) const = 0;
        virtual void Close(class XmlDoc* doc) const = 0;
        virtual class XmlDoc* NewDoc() const = 0;
        virtual bool IsBinary(const wchar_t* xml) const = 0;
        virtual bool IsBinary(const unsigned char* mem, unsigned int size) const = 0;
    }; /* size: 0x0008 */
}

namespace v15
{
    // unknown difference between v14
    using v14::XmlCfgDef2;
    using v14::XmlConfigReader;
    using v14::XmlDoc;
    using v14::XmlElement;
    using v14::_XmlSaxHandler;
    using v14::XmlNode;
    using v14::XmlPieceReader;
    using v14::XmlReader;
    using v14::XmlReaderIO;
    using v14::XmlReaderLog;
    using v14::XmlTextNode;
}
