#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXChannelType.h"

namespace ASAM {
namespace FIBEX {

/**
 * @brief complexType CHANNEL-TYPE
 */
class ASAM_FIBEX_EXPORT FRChannelType : public FXChannelType
{
public:
    FRChannelType(FXFibex *fibex = nullptr);

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /**
     * @brief FLEXRAY-CHANNEL-NAME
     *
     * Name of the channel.
     */
    QString m_flexrayChannelName;
};

} // FIBEX
} // ASAM
