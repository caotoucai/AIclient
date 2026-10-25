#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType INCLUDED-SIGNAL-TYPE
 *
 * Selected signal
 */
class ASAM_FIBEX_EXPORT FXIncludedSignalType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXIncludedSignalType(FXFibex *fibex = nullptr);

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element SIGNAL-INSTANCE-REF */
    QString m_signalInstanceRef;
};

} // FIBEX
} // ASAM
