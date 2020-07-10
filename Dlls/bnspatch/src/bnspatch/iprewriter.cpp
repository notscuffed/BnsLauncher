#include <sstream>
#include <memory>
#include <string>
#include <stdexcept>
#include <fnv1a.h>

#include "iprewriter.h"
#include "xmlpatch.h"
#include "logging.h"

std::vector<ippatch> g_ippatches;

USHORT htons(USHORT x)
{
    return x << 8 | x >> 8;
}

USHORT ntohs(USHORT x)
{
    return x << 8 | x >> 8;
}

ULONG str_to_ip(LPCWSTR str)
{
    ULONG address;
    int byte = 0;
    int i, j;

    for (i = 0; i < 4; i++)
    {
        for (j = 0; j < 3 && isdigit(str[j]); j++)
        {
            byte = byte * 10 + str[j] - '0';
        }

        if (j == 0 || (j > 1 && str[0] == '0') || byte > 255)
            return 0;

        (&address)[i] = byte;

        if (str[j] == 0 && i == 3)
            return address;

        if (str[j] != '.')
            return 0;
    }

    return address;
}

std::string ip_to_str(ULONG ip_address)
{
    std::stringstream ipsstr;
    uint8_t* ip = reinterpret_cast<uint8_t*>(&ip_address);
    ipsstr << (int)ip[0] << "." << (int)ip[1] << "." << (int)ip[2] << "." << (int)ip[3];
    return ipsstr.str();
}

void patch_ip(ULONG& ip, USHORT& port)
{
    ULONG old_ip = ip;
    USHORT old_port = port;

    for (auto& ip_patch : g_ippatches)
    {
        if (ip_patch.from_ip == ip && (ip_patch.from_port == port || ip_patch.from_port == 0))
        {
            ip = ip_patch.to_ip;

            if (ip_patch.to_port > 0)
            {
                port = ip_patch.to_port;
            }

            std::string old_ip_str = ip_to_str(old_ip);
            std::string ip_str = ip_to_str(ip);

            logfmt("%s:%d -> %s:%d",
                old_ip_str.c_str(),
                ntohs(old_port),
                ip_str.c_str(),
                ntohs(port));

            return;
        }
    }
}

void load_iprewriter_patches()
{
    log("Loading ip rewriter patches");

    auto patches = get_or_load_patches().child(L"patches");

    if (patches == nullptr) {
        log("Failed to get root element: patches");
        return;
    }

    for (auto& node : patches.children(L"rewrite-ip")) {
        ippatch patch{};

        // From ip
        auto fromIp = node.attribute(L"from-ip");
        if (fromIp == nullptr) {
            log("Required attribute 'from-ip' is missing from <rewrite-ip/>");
            continue;
        }

        patch.from_ip = str_to_ip(fromIp.value());

        if (patch.from_ip == 0) {
            logfmt("Invalid ip address inside 'from-ip' attribute: %s", fromIp.value());
            continue;
        }

        // From port
        auto fromPort = node.attribute(L"from-port");
        if (fromPort != nullptr) {
            patch.from_port = htons(_wtoi(fromPort.value()));
        }

        // To ip
        auto toIp = node.attribute(L"to-ip");
        if (toIp == nullptr) {
            log("Required attribute 'to-ip' is missing from <rewrite-ip/>");
            continue;
        }

        patch.to_ip = str_to_ip(toIp.value());

        if (patch.to_ip == 0) {
            logfmt("Invalid ip address inside 'to-ip' attribute: %s", toIp.value());
            continue;
        }

        // To port
        auto toPort = node.attribute(L"to-port");
        if (toPort != nullptr) {
            patch.to_port = htons(_wtoi(toPort.value()));
        }

        g_ippatches.push_back(patch);
    }
};
